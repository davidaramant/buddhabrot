using System;
using System.Drawing;
using System.IO;
using System.Threading;
using Buddhabrot.Core;
using Buddhabrot.EdgeSpans;
using Buddhabrot.IterationKernels;
using Buddhabrot.PointGrids;
using Buddhabrot.Points;
using PowerArgs;

namespace Buddhabrot
{
    class Program
    {
        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();
            Args.InvokeAction<BuddhabrotProgram>(args);
        }

        [ArgExceptionBehavior(ArgExceptionPolicy.StandardExceptionHandling)]
        public sealed class BuddhabrotProgram
        {
            private readonly CancellationToken _token;
            public BuddhabrotProgram()
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
                _token = cts.Token;
            }

            [HelpHook, ArgShortcut("-?"), ArgDescription("Shows this help")]
            public bool Help { get; set; }

            [ArgActionMethod, ArgDescription("Computes a point grid.")]
            public void PointGrid_Compute(
                [ArgDescription("The resolution (it will be squared)."), ArgRequired] int resolution,
                [ArgDescription("Computation type."), ArgDefaultValue(ComputationType.ScalarDouble)] ComputationType computationType,
                [ArgDescription("The directory for the resulting grid file."), ArgExistingDirectory, ArgDefaultValue(".")] string outputPath)
            {
                var viewPort = new ViewPort(
                    Constant.RenderingArea.GetPositiveImagArea(),
                    new Size(resolution, resolution / 2));

                PointGrid.Compute(
                    Path.Combine(outputPath, $"pointGrid_{resolution}_{computationType}"),
                    viewPort,
                    computationType,
                    _token);
            }

            [ArgActionMethod, ArgDescription("Plots a point grid.")]
            public void PointGrid_Plot(
                [ArgDescription("Input point grid file."), ArgRequired, ArgExistingFile] string pointGridPath)
            {
                var imageFilePath = Path.Combine(
                    Path.GetDirectoryName(pointGridPath),
                    Path.GetFileNameWithoutExtension(pointGridPath) + ".png");

                PointGridVisualizer.Render(pointGridPath, imageFilePath);
            }

            [ArgActionMethod, ArgDescription("Finds line segments straddling the Mandelbrot set.")]
            public void EdgeSpans_Find(
                [ArgDescription("Input point grid file."), ArgRequired, ArgExistingFile] string pointGridPath)
            {
                var number = Path.GetFileNameWithoutExtension(pointGridPath).Remove(0, "pointGrid".Length);

                var edgeSpansFilePath = Path.Combine(
                    Path.GetDirectoryName(pointGridPath),
                     $"edgeSpans{number}");

                EdgeSpanLocator.FindEdgeSpans(pointGridPath, edgeSpansFilePath);
            }

            [ArgActionMethod, ArgDescription("Renders the edge spans to an image.")]
            public void EdgeSpans_Plot(
                [ArgDescription("Edge spans file."), ArgRequired, ArgExistingFile] string edgeSpansFilePath,
                [ArgDescription("Shows the directions of the edges."), ArgDefaultValue(false)] bool showDirections)
            {
                var imageFilePath = Path.Combine(
                    Path.GetDirectoryName(edgeSpansFilePath),
                    Path.GetFileNameWithoutExtension(edgeSpansFilePath) + (showDirections ? ".withDirections" : "") + ".png");

                EdgeSpanVisualizer.RenderAllSpans(edgeSpansFilePath, imageFilePath, showDirections);
            }

            [ArgActionMethod, ArgDescription("Renders a single edge span to an image.")]
            public void EdgeSpans_PlotSingle(
                [ArgDescription("Edge spans file."), ArgRequired, ArgExistingFile] string edgeSpansFilePath,
                [ArgDescription("The index of the edge span to visualize.  If negative, a random one will be chosen."), ArgDefaultValue(-1)] int index,
                [ArgDescription("Resolution of the image (it will be squared)"), ArgDefaultValue(1024)] int sideResolution)
            {
                EdgeSpanVisualizer.RenderSingleSpan(edgeSpansFilePath, index, sideResolution);
            }

            [ArgActionMethod, ArgDescription("Finds border points from the edge spans.")]
            public void BorderPoints_CreateCsv(
                [ArgDescription("Edge spans file."), ArgRequired, ArgExistingFile] string edgeSpansFilePath,
                [ArgDescription("Number of edge spans to process.  Leave negative to do all of them."), ArgDefaultValue(-1)] int edgeSpanCount)
            {
                BorderPointsLocator.CalculateToCsv(edgeSpansFilePath, edgeSpanCount);
            }

            //[ArgActionMethod, ArgDescription("Finds points.")]
            //public void FindPoints(
            //    [ArgDescription("Input edges file."), ArgRequired, ArgExistingFile] string inputEdgesFilePath,
            //    [ArgDescription("Output directory."), ArgDefaultValue(".")] string outputDirectory)
            //{
            //    Console.WriteLine("Press Ctrl-C to exit...");

            //    if (!Directory.Exists(outputDirectory))
            //    {
            //        Directory.CreateDirectory(outputDirectory);
            //    }

            //    using (var finder = new PointFinder(inputEdgesFilePath,
            //        Path.Combine(outputDirectory, $"points{DateTime.Now:yyyyMMdd-HHmmss}")))
            //    {
            //        var cts = new CancellationTokenSource();
            //        Console.CancelKeyPress += (s, e) =>
            //        {
            //            if (!cts.IsCancellationRequested)
            //            {
            //                e.Cancel = true;
            //                cts.Cancel();
            //                Console.WriteLine("Cancelation requested...");
            //            }
            //        };

            //        finder.Start(cts.Token).Wait();
            //    }
            //}

            //[ArgActionMethod, ArgDescription("Validates that the points escape in the range.")]
            //public void ValidatePoints(
            //    [ArgDescription("Input points file."), ArgRequired, ArgExistingFile] string inputPointsFile,
            //    string kernelType,
            //    int maxIterations,
            //    [ArgDefaultValue(false)]bool showDetails)
            //{
            //    var cts = new CancellationTokenSource();

            //    var timer = Stopwatch.StartNew();

            //    IKernel PickKernel(string name)
            //    {
            //        switch (name.Trim().ToLowerInvariant())
            //        {
            //            case "scalar":
            //                return new ScalarKernel();
            //            case "vector":
            //                return new VectorKernel();
            //            case "opencl":
            //                return new KernelBuilder().BuildOpenCL();
            //            default:
            //                throw new ArgumentException("Unknown kernel type: " + name);
            //        }
            //    }

            //    using (var kernel = PickKernel(kernelType))
            //    {
            //        Console.WriteLine($"{kernel.GetType().Name} with {maxIterations:N0} iterations.");

            //        var batch = kernel.GetBatch();
            //        foreach (var point in PointReader.ReadPoints(inputPointsFile))//.Take(batch.Capacity))
            //        {
            //            batch.AddPoint(point);
            //        }

            //        var results = batch.ComputeIterations(cts.Token, maxIterations);

            //        if (showDetails)
            //        {
            //            for (int i = 0; i < results.Count; i++)
            //            {
            //                var inRange = Constant.IterationRange.IsInside(results.GetIteration(i));
            //                var iterationResult = inRange ? "" : "   NOT IN RANGE";

            //                var c = results.GetPoint(i);

            //                string p(double d) => d.ToString("+0.000000000000000;-0.000000000000000");

            //                Console.WriteLine(
            //                    $"{i:00}: {p(c.Real)} {p(c.Imaginary)}i\t{results.GetIteration(i),10:N0}{iterationResult}");
            //            }
            //        }
            //        else
            //        {
            //            Console.WriteLine($"Points outside range: {results.GetAllResults().Count(result => !Constant.IterationRange.IsInside(result.iterations))}");
            //        }
            //    }
            //    timer.Stop();
            //    Console.WriteLine($"Took {timer.Elapsed.Humanize(2)}");
            //}
        }
    }
}
