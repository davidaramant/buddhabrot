using System;
using System.Drawing;
using System.IO;
using System.Threading;
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
                [ArgDescription("The path for the resulting edges file."), ArgRequired] string outputFilePath,
                [ArgDescription("The resolution (it will be squared)."), ArgRequired] int resolution)
            {
                Edges.EdgeLocator.FindEdges(
                    Path.GetFullPath(outputFilePath),
                    Constant.RenderingArea,
                    new Size(resolution, resolution),
                    Constant.IterationRange
                );
            }

            [ArgActionMethod, ArgDescription("Renders the edge areas to an image.  The image location/name will be based on the edge file.")]
            public void VisualizeEdges(
                [ArgDescription("Edges file."), ArgRequired, ArgExistingFile] string edgesFilePath)
            {
                var imageFilePath = Path.Combine(
                    Path.GetDirectoryName(edgesFilePath),
                    Path.GetFileNameWithoutExtension(edgesFilePath) + ".png");

                Edges.EdgeVisualizer.Render(edgesFilePath, imageFilePath);
            }

            [ArgActionMethod, ArgDescription("Finds and renders the edge areas.")]
            public void FindAndVisualizeEdges(
                [ArgDescription("The path for the resulting edges file."), ArgRequired] string edgesFilePath,
                [ArgDescription("The resolution (it will be squared)."), ArgRequired] int resolution)
            {
                FindEdges(edgesFilePath, resolution);
                VisualizeEdges(edgesFilePath);
            }

            [ArgActionMethod, ArgDescription("Combines adjacent edge areas.")]
            public void CompressEdges(
                [ArgDescription("Input edges file."), ArgRequired, ArgExistingFile] string inputEdgesFilePath,
                [ArgDescription("Output edges file."), ArgRequired] string outputEdgesFilePath)
            {
                var areas = Edges.EdgeAreas.Load(inputEdgesFilePath);
                var compressedAreas = areas.CreateCompressedVersion();
                compressedAreas.Write(outputEdgesFilePath);
            }

            [ArgActionMethod, ArgDescription("Finds points.")]
            public void FindPoints(
                [ArgDescription("Input edges file."), ArgRequired, ArgExistingFile] string inputEdgesFilePath,
                [ArgDescription("Output directory."), ArgRequired] string outputDirectory)
            {
                System.Console.WriteLine("Press Ctrl-C to exit...");

                if (!Directory.Exists(outputDirectory))
                {
                    Directory.CreateDirectory(outputDirectory);
                }

                using (var statistics = new PointFinderStatistics())
                {
                    var areas = Edges.EdgeAreas.Load(inputEdgesFilePath);
                    var randomNumbers = new RandomPointGenerator(areas.GetDistributedComplexAreas());
                    var writer = new PointWriter(Path.Combine(outputDirectory, $"points{DateTime.Now:yyyyMMdd-HHmmss}"));

                    var cts = new CancellationTokenSource();
                    System.Console.CancelKeyPress += (s, e) =>
                    {
                        if (!cts.IsCancellationRequested)
                        {
                            e.Cancel = true;
                            cts.Cancel();
                            System.Console.WriteLine("Cancelation requested...");
                        }
                    };

                    using (var finder = new IntelGpuOpenCLPointFinder(randomNumbers, Constant.IterationRange, writer, statistics))
                    {
                        finder.Start(cts.Token).Wait();
                    }
                }
            }
        }
    }
}
