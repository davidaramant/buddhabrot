using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Buddhabrot.Extensions;
using Buddhabrot.Images;
using Buddhabrot.Utility;
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
            using (var timer = TimedOperation.Start("Plotting point grid", totalWork: grid.PointResolution.Area()))
            {
                var image = new FastImage(grid.PointResolution);

                image.Fill(Color.White);

                Parallel.ForEach(grid, row =>
                {
                    var y = grid.PointResolution.Height - row.Y - 1;

                    foreach (var setSegment in row.GetSegmentsInSet())
                    {
                        foreach (var x in Enumerable.Range(setSegment.StartCol, setSegment.Length))
                        {
                            image.SetPixel(x, y, Color.Black);
                        }
                    }
                    timer.AddWorkDone(grid.PointResolution.Width);
                });

                image.Save(imageFilePath);
            }
        }
    }
}
