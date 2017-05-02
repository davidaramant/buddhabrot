using System.Drawing;
using Buddhabrot.Core;
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
                    System.IO.Path.GetFullPath(outputFilePath),
                    Constant.RenderingArea,
                    new Size(32, 32), //Constant.EdgeGridResolution,
                    new IterationRange(0, 1000) // Constant.IterationRange
                );

            }
        }
    }
}
