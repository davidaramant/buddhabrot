using System.Drawing;
using Buddhabrot.Images;
using log4net;

namespace Buddhabrot.PointGrids
{
    static class PointGridVisualizer
    {
        private static readonly ILog Log = LogManager.GetLogger(nameof(PointGridVisualizer));

        public static void Render(string gridFilePath, string imageFilePath)
        {
            Log.Info($"Output image: {imageFilePath}");

            using (var grid = PointGrid.Load(gridFilePath))
            {
                var image = new FastImage(grid.PointResolution);

                image.Fill(Color.White);

                foreach (var row in grid)
                {
                    int x = 0;
                    var y = grid.PointResolution.Height - row.Y - 1;
                    foreach (var inSet in row)
                    {
                        image.SetPixel(x, y, inSet ? Color.Black : Color.White);

                        x++;
                    }
                }

                image.Save(imageFilePath);
            }
        }
    }
}
