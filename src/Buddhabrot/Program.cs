using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Buddhabrot.IterationKernels;
using Buddhabrot.Points;
using Humanizer;
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
                [ArgDescription("Input points file."), ArgRequired, ArgExistingFile] string inputPointsFile,
                string kernelType,
                int maxIterations,
                [ArgDefaultValue(false)]bool showDetails)
            {
                var cts = new CancellationTokenSource();

                var timer = Stopwatch.StartNew();

                IKernel PickKernel(string name)
                {
                    switch (name.Trim().ToLowerInvariant())
                    {
                        case "scalar":
                            return new ScalarKernel();
                        case "vector":
                            return new VectorKernel();
                        case "opencl":
                            return new KernelBuilder().BuildOpenCL();
                        default:
                            throw new ArgumentException("Unknown kernel type: " + name);
                    }
                }

                using (var kernel = PickKernel(kernelType))
                {
                    Console.WriteLine($"{kernel.GetType().Name} with {maxIterations:N0} iterations.");

                    var batch = kernel.GetBatch();
                    foreach (var point in PointReader.ReadPoints(inputPointsFile).Take(batch.Capacity))
                    {
                        batch.AddPoint(point);
                    }

                    var results = batch.ComputeIterations(cts.Token, maxIterations);

                    if (showDetails)
                    {
                        for (int i = 0; i < results.Count; i++)
                        {
                            var inRange = Constant.IterationRange.IsInside(results.GetIteration(i));
                            var iterationResult = inRange ? "" : "   NOT IN RANGE";

                            var c = results.GetPoint(i);

                            string p(double d) => d.ToString("+0.000000000000000;-0.000000000000000");

                            Console.WriteLine(
                                $"{i:00}: {p(c.Real)} {p(c.Imaginary)}i\t{results.GetIteration(i),10:N0}{iterationResult}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Points outside range: {results.GetAllResults().Count(result => !Constant.IterationRange.IsInside(result.iterations))}");
                    }
                }
                timer.Stop();
                Console.WriteLine($"Took {timer.Elapsed.Humanize(2)}");
            }
        }
    }
}
