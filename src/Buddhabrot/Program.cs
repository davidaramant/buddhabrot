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

            [ArgActionMethod, ArgDescription("Renders the edge areas to an image.  The image location/name will be based on the edge file.")]
            public void VisualizeEdges(
                [ArgDescription("Edges file."), ArgRequired] string edgesFilePath)
            {
                var imageFilePath = Path.Combine(
                    Path.GetDirectoryName(edgesFilePath),
                    Path.GetFileNameWithoutExtension(edgesFilePath) + ".png");

                Edges.EdgeVisualizer.Render(edgesFilePath, imageFilePath);
            }

            [ArgActionMethod, ArgDescription("Finds and renders the edge areas.")]
            public void FindAndVisualizeEdges(
                [ArgDescription("The path for the resulting edges file."), ArgRequired] string edgesFilePath)
            {
                FindEdges(edgesFilePath);
                VisualizeEdges(edgesFilePath);
            }

            [ArgActionMethod, ArgDescription("Combines adjacent edge areas.")]
            public void CompressEdges(
                [ArgDescription("Input edges file."), ArgRequired] string inputEdgesFilePath,
                [ArgDescription("Output edges file."), ArgRequired] string outputEdgesFilePath)
            {
                var areas = Edges.EdgeAreas.Load(inputEdgesFilePath);
                var compressedAreas = areas.Compress();
                compressedAreas.Write(outputEdgesFilePath);
            }
        }
    }
}
