using System.IO;
using PowerArgs;

namespace Buddhabrot
{
    class Program
    {
        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();
            Args.InvokeAction<BudhabrotProgram>(args);
        }

        [ArgExceptionBehavior(ArgExceptionPolicy.StandardExceptionHandling)]
        public sealed class BudhabrotProgram
        {
            [HelpHook, ArgShortcut("-?"), ArgDescription("Shows this help")]
            public bool Help { get; set; }

            [ArgActionMethod, ArgDescription("Finds the border regions of the Mandelbrot set.")]
            public void FindEdges([ArgDescription("The path for the resulting edges file."), ArgRequired] string outputFilePath)
            {
                Edges.EdgeLocator.FindEdges(
                    Path.GetFullPath(outputFilePath),
                    Constant.RenderingArea,
                    Constant.EdgeGridResolution,
                    Constant.IterationRange
                );
            }

            [ArgActionMethod, ArgDescription("Renders the edge areas to an image.")]
            public void VisualizeEdges(
                [ArgDescription("Edges file."), ArgRequired] string edgesFilePath,
                [ArgDescription("Output image (optional).  Defaults to a PNG file based on the edges file name/location.")] string imageFilePath)
            {
                if (string.IsNullOrWhiteSpace(imageFilePath))
                {
                    imageFilePath = Path.Combine(
                        Path.GetDirectoryName(edgesFilePath),
                        Path.GetFileNameWithoutExtension(edgesFilePath) + ".png");
                }

                Edges.EdgeVisualizer.Render(edgesFilePath, imageFilePath);
            }
        }
    }
}
