using System;
using System.Drawing;
using Buddhabrot.Extensions;
using Buddhabrot.Images;
using log4net;

namespace Buddhabrot.Edges
{
    static class CornerVisualizer
    {
        private static readonly ILog Log = LogManager.GetLogger(nameof(CornerVisualizer));

        public static void Render(string edgesFilePath, string imageFilePath)
        {
            Log.Info($"Loading file: {edgesFilePath}, output image: {imageFilePath}");

            const int scale = 4;

            var edgeAreaColor = Color.Gray;
            var inSetColor = Color.Red;
            var outsideSetColor = Color.Black;

            using (var edgeAreas = EdgeAreas.Load(edgesFilePath))
            {
                var imageResolution = edgeAreas.GridResolution.Scale(scale);
                if (imageResolution.Area() > 500_000_000)
                {
                    throw new ArgumentException("Edges are too fine to plot corners.");
                }
                var image = new FastImage(imageResolution);

                image.Fill(Color.White);
                foreach (var edgeArea in edgeAreas.GetAreas())
                {
                    // the grid locations are relative to the the real axis on the positive side
                    var logicalEdgeLocation = new Point(
                        edgeArea.GridLocation.X,
                        edgeAreas.GridResolution.Height - edgeArea.GridLocation.Y - 1);

                    for (int boxHeight = 0; boxHeight < scale; boxHeight++)
                    {
                        for (int boxWidth = 0; boxWidth < scale; boxWidth++)
                        {
                            image.SetPixel(
                                logicalEdgeLocation.X * scale + boxWidth,
                                logicalEdgeLocation.Y * scale + boxHeight,
                                edgeAreaColor);
                        }
                    }

                    // The Y coordinates are flipped
                    Point GetCornerLocation(Corners corner)
                    {
                        switch (corner)
                        {
                            case Corners.BottomLeft:
                                return new Point(
                                    logicalEdgeLocation.X * scale,
                                    logicalEdgeLocation.Y * scale + scale - 1);
                            case Corners.BottomRight:
                                return new Point(
                                    logicalEdgeLocation.X * scale + scale - 1,
                                    logicalEdgeLocation.Y * scale + scale - 1);
                            case Corners.TopLeft:
                                return new Point(
                                    logicalEdgeLocation.X * scale,
                                    logicalEdgeLocation.Y * scale);
                            case Corners.TopRight:
                                return new Point(
                                    logicalEdgeLocation.X * scale + scale - 1,
                                    logicalEdgeLocation.Y * scale);
                            default:
                                throw new ArgumentException();
                        }
                    }

                    foreach (var corner in new[] { Corners.BottomLeft, Corners.BottomRight, Corners.TopLeft, Corners.TopRight, })
                    {
                        image.SetPixel(
                            GetCornerLocation(corner),
                            edgeArea.CornersInSet.HasFlag(corner) ? inSetColor : outsideSetColor);
                    }
                }
                image.Save(imageFilePath);
            }
        }
    }
}
