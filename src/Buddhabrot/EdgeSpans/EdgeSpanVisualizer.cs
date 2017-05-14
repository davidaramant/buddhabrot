using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Buddhabrot.Core;
using Buddhabrot.Extensions;
using Buddhabrot.Images;
using Buddhabrot.IterationKernels;
using Buddhabrot.PointGrids;
using Buddhabrot.Utility;
using log4net;

namespace Buddhabrot.EdgeSpans
{
    static class EdgeSpanVisualizer
    {
        private static readonly ILog Log = LogManager.GetLogger(nameof(EdgeSpanVisualizer));

        public static void RenderAllSpans(string edgeSpansPath, string imageFilePath, bool showDirections)
        {
            Log.Info($"Output image: {imageFilePath}");
            if (showDirections)
            {
                Log.Info("Displaying span directions as well.");
            }

            using (var spans = EdgeSpanStream.Load(edgeSpansPath))
            {
                var scale = showDirections ? 3 : 1;

                var resolution = spans.PointResolution.Scale(scale);

                var image = new FastImage(resolution);

                image.Fill(Color.White);

                Point CorrectLocation(Point p) => new Point(p.X, resolution.Height - p.Y - 1);

                Parallel.ForEach(spans, locatedSpan =>
                {
                    if (showDirections)
                    {
                        var scaledLocation = locatedSpan.Location.Scale(scale);
                        for (int yDelta = 0; yDelta < 3; yDelta++)
                        {
                            for (int xDelta = 0; xDelta < 3; xDelta++)
                            {
                                image.SetPixel(
                                    CorrectLocation(scaledLocation.OffsetBy(xDelta, yDelta)),
                                    (locatedSpan.Location.X + locatedSpan.Location.Y) % 2 == 1 ? Color.Black : Color.Gray);
                            }
                        }
                    }
                    else
                    {
                        image.SetPixel(
                            CorrectLocation(locatedSpan.Location),
                            Color.Black);
                    }
                });

                if (showDirections)
                {
                    Parallel.ForEach(spans, locatedSpan =>
                    {
                        var scaledLocation = locatedSpan.Location.Scale(scale);
                        var pointingTo = scaledLocation.OffsetBy(1, 1).OffsetIn(locatedSpan.Direction);

                        image.SetPixel(
                            CorrectLocation(pointingTo),
                            Color.Red);

                    });
                }

                image.Save(imageFilePath);
            }
        }

        public static void RenderSingleSpan(string edgeSpansPath, int spanIndex)
        {
            var resolution = new Size(1024, 1024);

            using (var spans = EdgeSpanStream.Load(edgeSpansPath))
            using (var timer = TimedOperation.Start("Rendering edge span", reportProgress: true, totalWork: resolution.Area()))
            {
                var random = new Random();

                var index = spanIndex >= 0 ? spanIndex : random.Next(0, spans.Count);

                var imageFilePath = Path.Combine(
                    Path.GetDirectoryName(edgeSpansPath),
                    Path.GetFileNameWithoutExtension(edgeSpansPath) + $"_{index}.png");

                Log.Info($"Using edge span index {index:N0}");
                Log.Info($"Output file: {imageFilePath}");

                var span = spans.ElementAt(index).Span;

                Log.Info($"Edge span: {span}");

                var image = new FastImage(resolution);

                var viewPort = GetEdgeSpanViewPort(span);
                Log.Info($"View port: {viewPort}");

                var positionCalculator = new PositionCalculator(resolution, viewPort);

                Point CorrectLocation(Point p) => new Point(p.X, resolution.Height - p.Y - 1);

                var spanLength = span.Length;
                var highlightSize = spanLength * 0.01;
                var highlightSizeSquared = highlightSize * highlightSize;

                for (int row = 0; row < resolution.Height; row++)
                {
                    Parallel.For(0, resolution.Width,
                        col =>
                        {
                            var position = new Point(col, row);

                            var c = positionCalculator.GetPoint(position);
                            var imagePosition = CorrectLocation(position);


                            Color PickColor()
                            {
                                if ((c - span.InSet).MagnitudeSquared() < highlightSizeSquared)
                                    return Color.Red;

                                if ((c - span.NotInSet).MagnitudeSquared() < highlightSizeSquared)
                                    return Color.Green;

                                var isInSet = ScalarDoubleKernel.FindEscapeTime(c).IsInfinite;
                                return isInSet ? Color.Black : Color.White;
                            }


                            image.SetPixel(imagePosition, PickColor());
                        });

                    timer.AddWorkDone(resolution.Width);
                }

                image.Save(imageFilePath);
            }
        }

        private static ComplexArea GetEdgeSpanViewPort(EdgeSpan span)
        {
            var middle = span.GetMidPoint();

            var leftMost = Math.Min(span.NotInSet.Real, span.InSet.Real);
            var rightMost = Math.Max(span.NotInSet.Real, span.InSet.Real);
            var bottomMost = Math.Min(span.NotInSet.Imaginary, span.InSet.Imaginary);
            var topMost = Math.Max(span.NotInSet.Imaginary, span.InSet.Imaginary);

            var horizontalDistance = rightMost - leftMost;
            var verticalDistance = topMost - bottomMost;

            var horizontalLongest = horizontalDistance > verticalDistance;

            var paddingPercentage = 0.1;

            var sideLength = horizontalLongest ? horizontalDistance : verticalDistance;
            var paddingLength = paddingPercentage * sideLength;
            var paddedLength = sideLength + 2 * paddingLength;

            var halfPaddedLength = paddedLength / 2;

            return new ComplexArea(
                realRange: new DoubleRange(middle.Real - halfPaddedLength, middle.Real + halfPaddedLength),
                imagRange: new DoubleRange(middle.Imaginary - halfPaddedLength,
                    middle.Imaginary + halfPaddedLength));
        }
    }
}
