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
            using (var timer = TimedOperation.Start("points", totalWork: grid.ViewPort.Resolution.Area()))
            {
                var image = new FastImage(grid.ViewPort.Resolution);

                image.Fill(Color.White);

                Parallel.ForEach(grid, row =>
                {
                    foreach (var setSegment in row.GetSegmentsInSet())
                    {
                        foreach (var x in Enumerable.Range(setSegment.StartCol, setSegment.Length))
                        {
                            image.SetPixel(x, row.Y, Color.Black);
                        }
                    }
                    timer.AddWorkDone(grid.ViewPort.Resolution.Width);
                });

                image.Save(imageFilePath);
            }
        }
    }
}
