using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Buddhabrot.IterationKernels;
using Buddhabrot.Points;
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
            public void FindEdges(
                [ArgDescription("The resolution (it will be squared)."), ArgRequired] int resolution,
                [ArgDescription("The directory for the resulting edges file."), ArgExistingDirectory, ArgDefaultValue(".")] string outputPath)
            {
                Edges.EdgeLocator.FindEdges(
                    Path.Combine(outputPath, $"edges{resolution}"),
                    Constant.RenderingArea,
                    resolution,
                    Constant.IterationRange
                );
            }

            [ArgActionMethod, ArgDescription("Renders the edge areas to an image.  The image location/name will be based on the edge file.")]
            public void PlotEdges(
                [ArgDescription("Edges file."), ArgRequired, ArgExistingFile] string edgesFilePath)
            {
                var imageFilePath = Path.Combine(
                    Path.GetDirectoryName(edgesFilePath),
                    Path.GetFileNameWithoutExtension(edgesFilePath) + ".png");

                Edges.EdgeVisualizer.Render(edgesFilePath, imageFilePath);
            }

            [ArgActionMethod, ArgDescription("Renders the edge areas with corners highlighted to an image.  The image location/name will be based on the edge file.")]
            public void PlotCorners(
                [ArgDescription("Edges file."), ArgRequired, ArgExistingFile] string edgesFilePath)
            {
                var imageFilePath = Path.Combine(
                    Path.GetDirectoryName(edgesFilePath),
                    Path.GetFileNameWithoutExtension(edgesFilePath) + ".corners.png");

                Edges.CornerVisualizer.Render(edgesFilePath, imageFilePath);
            }

            [ArgActionMethod, ArgDescription("Finds and renders the edge areas.")]
            public void FindAndPlotEdges(
                [ArgDescription("The resolution (it will be squared)."), ArgRequired] int resolution,
                [ArgDescription("The directory for the resulting edges file."), ArgExistingDirectory, ArgDefaultValue(".")] string outputPath)
            {
                FindEdges(resolution, outputPath);
                PlotEdges(Path.Combine(outputPath, $"edges{resolution}"));
            }

            [ArgActionMethod, ArgDescription("Finds points.")]
            public void FindPoints(
                [ArgDescription("Input edges file."), ArgRequired, ArgExistingFile] string inputEdgesFilePath,
                [ArgDescription("Output directory."), ArgDefaultValue(".")] string outputDirectory)
            {
                Console.WriteLine("Press Ctrl-C to exit...");

                if (!Directory.Exists(outputDirectory))
                {
                    Directory.CreateDirectory(outputDirectory);
                }

                using (var finder = new PointFinder(inputEdgesFilePath,
                    Path.Combine(outputDirectory, $"points{DateTime.Now:yyyyMMdd-HHmmss}")))
                {
                    var cts = new CancellationTokenSource();
                    Console.CancelKeyPress += (s, e) =>
                    {
                        if (!cts.IsCancellationRequested)
                        {
                            e.Cancel = true;
                            cts.Cancel();
                            Console.WriteLine("Cancelation requested...");
                        }
                    };

                    finder.Start(cts.Token).Wait();
                }
            }

            [ArgActionMethod, ArgDescription("Validates that the points escape in the range.")]
            public void ValidatePoints(
                [ArgDescription("Input points file."), ArgRequired, ArgExistingFile] string inputPointsFile)
            {
                var cts = new CancellationTokenSource();
                using (var kernel = new VectorKernel())
                {
                    var batch = kernel.GetBatch();
                    foreach (var point in PointReader.ReadPoints(inputPointsFile).Take(batch.Capacity))
                    {
                        batch.AddPoint(point);
                    }

                    var results = batch.ComputeIterations(cts.Token);
                    for (int i = 0; i < results.Count; i++)
                    {
                        Console.WriteLine($"{i} = Point: {results.GetPoint(i)}, iterations: {results.GetIteration(i):N0}");
                    }
                }
            }
        }
    }
}
