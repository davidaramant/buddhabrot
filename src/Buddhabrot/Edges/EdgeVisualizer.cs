using System.Drawing;
using Buddhabrot.Images;
using log4net;

namespace Buddhabrot.Edges
{
    static class EdgeVisualizer
    {
        private static readonly ILog Log = LogManager.GetLogger(nameof(EdgeVisualizer));

        public static void Render(string edgesFilePath, string imageFilePath)
        {
            Log.Info($"Loading file: {edgesFilePath}, output image: {imageFilePath}");

            using (var edgeAreas = EdgeAreas.Load(edgesFilePath))
            {
                var image = new FastImage(edgeAreas.GridResolution);

                image.Fill(Color.White);
                foreach (var location in edgeAreas.GetAreaLocations())
                {
                    // the grid locations are relative to the the real axis on the positive side
                    image.SetPixel(location.X, edgeAreas.GridResolution.Height - location.Y - 1, Color.Black);
                }
                image.Save(imageFilePath);
            }
        }
    }
}

