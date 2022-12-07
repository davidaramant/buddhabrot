using Buddhabrot.Core.Boundary;
using Buddhabrot.Core.DataStorage;
using Spectre.Console;

namespace BoundaryFinder;

class Program
{
    public static int Main(string[] args)
    {
        if (args.Length != 2 ||
            !int.TryParse(args[0], out var power) ||
            !int.TryParse(args[1], out var limitMillions))
        {
            Console.WriteLine("Arguments: power limit");
            Console.WriteLine(" - Power is the vertical power");
            Console.WriteLine(" - Limit is the max iteration limit in millions (IE 5 = 5,000,000)");
            return -1;
        }

        var limit = limitMillions * 1_000_000;

        var boundaryParameters = new BoundaryParameters(new AreaDivisions(power), limit);

        AnsiConsole.MarkupLine(boundaryParameters.ToString());

        AnsiConsole.Progress()
            .Columns(
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new ElapsedTimeColumn()
            )
            .Start(ctx =>
            {
                var boundaryTask = ctx.AddTask("Visiting border");
                var borderTask = ctx.AddTask("Extracting border regions");
                borderTask.StopTask();
                var transformTask = ctx.AddTask($"Transforming {nameof(VisitedRegions)} to {nameof(RegionLookup)}");
                transformTask.StopTask();

                boundaryTask.IsIndeterminate = true;

                var visitedRegions = new VisitedRegions(capacity: boundaryParameters.Divisions.QuadrantDivisions * 2);

                BoundaryCalculator.VisitBoundary(boundaryParameters, visitedRegions, CancellationToken.None);

                boundaryTask.StopTask();
                
                borderTask.IsIndeterminate = true;

                var boundaryRegions = visitedRegions.GetBoundaryRegions();

                borderTask.StopTask();

                transformTask.IsIndeterminate = true;

                var transformer = new VisitedRegionsToRegionLookup(visitedRegions);
                var lookup = transformer.Transform();

                transformTask.StopTask();

                var defaultDataSetPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "Buddhabrot",
                    "Mandelbrot Set Boundaries");

                var dataProvider = new DataProvider()
                {
                    DataStoragePath = defaultDataSetPath
                };

                 dataProvider.SaveBoundaryRegions(
                     boundaryParameters,
                     boundaryRegions,
                     lookup);
                 
                 AnsiConsole.MarkupLine($"Found {boundaryRegions.Count:N0} boundary regions");
                 AnsiConsole.MarkupLine($"{nameof(VisitedRegions)} nodes: {visitedRegions.NodeCount:N0}");
                 AnsiConsole.MarkupLine($"{nameof(RegionLookup)} nodes: {lookup.NodeCount:N0} ({(double)lookup.NodeCount/visitedRegions.NodeCount:P0})");
            });
        
        return 0;
    }
}