using System.Diagnostics;
using Buddhabrot.Core.Boundary;
using Buddhabrot.Core.DataStorage;
using Buddhabrot.Core.Utilities;
using Humanizer;
using Spectre.Console;

namespace BoundaryFinder;

class Program
{
    private sealed record Metrics(
        TimeSpan Duration,
        int BorderRegions,
        int VisitedRegionNodes,
        int RegionLookupNodes)
    {
        public double NormalizedSize => (double)RegionLookupNodes / VisitedRegionNodes;
    }

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

        //AnsiConsole.MarkupLine(boundaryParameters.ToString());

        Metrics? metrics = null;

        AnsiConsole.Status()
            .Spinner(Spinner.Known.SquareCorners)
            .Start("Finding boundary...", ctx =>
            {
                var timer = Stopwatch.StartNew();
                var visitedRegions = new VisitedRegions(capacity: boundaryParameters.Divisions.QuadrantDivisions * 2);

                BoundaryCalculator.VisitBoundary(boundaryParameters, visitedRegions, CancellationToken.None);

                var boundaryRegions = visitedRegions.GetBoundaryRegions();
                
                var transformer = new VisitedRegionsToRegionLookup(visitedRegions);
                var lookup = transformer.Transform();

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

                metrics = new Metrics(
                    Duration: timer.Elapsed,
                    BorderRegions: boundaryRegions.Count,
                    VisitedRegionNodes: visitedRegions.NodeCount,
                    RegionLookupNodes: lookup.NodeCount);
            });

        if (metrics == null)
            return -1;
        
        var table = new Table();

        table.AddColumn("Key");
        table.AddColumn(new TableColumn("Value").RightAligned());

        table.AddRow("Vertical Power", boundaryParameters.Divisions.VerticalPower.ToString());
        table.AddRow("Divisions", boundaryParameters.Divisions.QuadrantDivisions.ToString("N0"));
        table.AddRow("Iteration Limit", boundaryParameters.MaxIterations.ToString("N0"));
        table.AddRow("Timestamp", DateTime.Now.ToString("s"));

        var (os, cpu, ram) = ComputerDescription.GetInfo();
        table.AddRow("OS", os);
        table.AddRow("CPU", cpu);
        table.AddRow("RAM", ram);

        table.AddRow("Duration", metrics.Duration.Humanize(2));
        table.AddRow("Border regions", metrics.BorderRegions.ToString("N0"));
        table.AddRow($"{nameof(VisitedRegions)} nodes", metrics.VisitedRegionNodes.ToString("N0"));
        table.AddRow($"{nameof(RegionLookup)} nodes", metrics.RegionLookupNodes.ToString("N0"));
        table.AddRow($"Normalized size", metrics.NormalizedSize.ToString("P0"));

        AnsiConsole.Write(table);

        return 0;
    }
}