using System.Drawing;
using System.Threading.Tasks;
using Buddhabrot.Core;
using Buddhabrot.Extensions;
using Buddhabrot.Images;
using Buddhabrot.Utility;
using log4net;

namespace Buddhabrot.Points
{
    public static class PointVisualizer
    {
        private static readonly ILog Log = LogManager.GetLogger(nameof(PointVisualizer));

        public static void Render(string pointFilePath, Size? resolution)
        {
            using (var points = PointStream.Load(pointFilePath))
            {
                var viewPort = points.ViewPort;
                var actualResolution = resolution ?? points.ViewPort.Resolution;
                if (actualResolution != points.ViewPort.Resolution)
                {
                    viewPort = new ViewPort(viewPort.Area, actualResolution);
                }

                var imageFilePath = pointFilePath + $".{actualResolution.Width}x{actualResolution.Height}.png";

                using (var timer = TimedOperation.Start("points", totalWork: actualResolution.Area()))
                {
                    var image = new FastImage(actualResolution);

                    image.Fill(Color.White);

                    Parallel.ForEach(points, point =>
                    {
                        image.SetPixel(viewPort.GetPosition(point), Color.Black);
                        timer.AddWorkDone(1);
                    });

                    image.Save(imageFilePath);
                }
            }
        }
    }
}
