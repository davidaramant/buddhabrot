using System.Drawing;
using System.Threading.Tasks;
using Buddhabrot.Core;
using Buddhabrot.Edges;
using Buddhabrot.Images;
using log4net;

namespace Buddhabrot.PointGrids
{
    static class PointGridVisualizer
    {
        private static readonly ILog Log = LogManager.GetLogger(nameof(PointGridVisualizer));

        public static void Render(Size resolution, ComplexArea viewPort, string imageFilePath)
        {
            Log.Info($"Output image: {imageFilePath}");

            var pointCalculator = new PositionCalculator(resolution, viewPort);

            var image = new FastImage(resolution);

            image.Fill(Color.White);
            for (int row = 0; row < resolution.Height; row++)
            {
                int y = resolution.Height - row - 1;

                Parallel.For(
                    0,
                    resolution.Width,
                    col => image.SetPixel(col, y,
                        MandelbrotChecker.FindEscapeTime(pointCalculator.GetPoint(col, row)).IsInfinite ? Color.Black : Color.White));
            }
            image.Save(imageFilePath);
        }
    }
}
