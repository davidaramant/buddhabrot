using System.Drawing;
using System.Threading.Tasks;
using Buddhabrot.Extensions;
using Buddhabrot.Images;
using log4net;

namespace Buddhabrot.EdgeSpans
{
    static class EdgeSpanVisualizer
    {
        private static readonly ILog Log = LogManager.GetLogger(nameof(EdgeSpanVisualizer));

        public static void Render(string edgeSpansPath, string imageFilePath, bool showDirections)
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
    }
}
