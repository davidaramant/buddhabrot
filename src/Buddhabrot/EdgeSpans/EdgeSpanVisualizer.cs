using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Buddhabrot.Images;
using log4net;

namespace Buddhabrot.EdgeSpans
{
    static class EdgeSpanVisualizer
    {
        private static readonly ILog Log = LogManager.GetLogger(nameof(EdgeSpanVisualizer));

        public static void Render(string edgeSpansPath, string imageFilePath)
        {
            Log.Info($"Output image: {imageFilePath}");

            using (var span = EdgeSpanStream.Load(edgeSpansPath))
            {
                var image = new FastImage(span.PointResolution);

                image.Fill(Color.White);

                Parallel.ForEach(span.Select(pair => pair.location), position =>
                {
                    var y = span.PointResolution.Height - position.Y - 1;
                    image.SetPixel(position.X, y, Color.Black);
                });

                image.Save(imageFilePath);
            }
        }
    }
}
