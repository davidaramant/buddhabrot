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
        int NumBorderRegions,
        int NumVisitedRegionNodes,
        int NumRegionLookupNodes)
    {
        public double NormalizedSize => (double)NumRegionLookupNodes / NumVisitedRegionNodes;
    }

    public static int Main(string[] args)
    {
        if (args.Length is < 2 or > 3 ||
            !int.TryParse(args[0], out var power) ||
            !int.TryParse(args[1], out var limitMillions))
        {
            Console.WriteLine("Arguments: power limit");
            Console.WriteLine(" - Power is the vertical power");
            Console.WriteLine(" - Limit is the max iteration limit in millions (IE 5 = 5,000,000)");
            return -1;
        }

        var note = args.Length == 3 ? args[2] : string.Empty;

        var limit = limitMillions * 1_000_000;

        var boundaryParameters = new BoundaryParameters(new AreaDivisions(power), limit);

        Metrics? metrics = null;

        AnsiConsole.Status()
            .Spinner(Spinner.Known.SquareCorners)
            .Start($"Finding boundary... (started {DateTime.Now:HH:mm:ss})", ctx =>
            {
                var timer = Stopwatch.StartNew();
                var visitedRegions = new VisitedRegions(capacity: boundaryParameters.Divisions.QuadrantDivisions * 2);

                BoundaryCalculator.VisitBoundary(boundaryParameters, visitedRegions, CancellationToken.None);

                var boundaryRegions = visitedRegions.GetBoundaryRegions();

                var transformer = new QuadTreeTransformer(visitedRegions);
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
                    NumBorderRegions: boundaryRegions.Count,
                    NumVisitedRegionNodes: visitedRegions.NodeCount,
                    NumRegionLookupNodes: lookup.NodeCount);
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
        table.AddRow("Duration", metrics.Duration.Humanize(2));
        table.AddRow("Border regions", metrics.NumBorderRegions.ToString("N0"));
        table.AddRow($"{nameof(VisitedRegions)} nodes", metrics.NumVisitedRegionNodes.ToString("N0"));
        table.AddRow($"{nameof(RegionLookup)} nodes", metrics.NumRegionLookupNodes.ToString("N0"));
        table.AddRow($"Normalized size", metrics.NormalizedSize.ToString("P0"));

        AnsiConsole.Write(table);

        if (!string.IsNullOrEmpty(note))
        {
            var csvPath = FindTimesCsv();

            var (os, cpu, ram) = ComputerDescription.GetInfo();

            File.AppendAllText(
                csvPath,
                Environment.NewLine + string.Join(',',
                    new[]
                    {
                        DateTime.Now.ToString("yyyy-MM-dd"),
                        DateTime.Now.ToString("HH:mm:ss"),
                        metrics.Duration.Humanize(2),
                        metrics.Duration.TotalSeconds.ToString("N0"),
                        os,
                        cpu,
                        ram,
                        boundaryParameters.Divisions.VerticalPower.ToString(),
                        boundaryParameters.Divisions.QuadrantDivisions.ToString("N0"),
                        boundaryParameters.MaxIterations.ToString("N0"),
                        metrics.NumBorderRegions.ToString("N0"),
                        metrics.NumVisitedRegionNodes.ToString("N0"),
                        metrics.NumRegionLookupNodes.ToString("N0"),
                        metrics.NormalizedSize.ToString("P0"),
                        note
                    }.Select(EscapeCsv)));
        }

        static string EscapeCsv(string value)
        {
            if (value.Contains('"') || value.Contains(','))
                return '"' + value.Replace("\"", new string('"', 3)) + '"';
            return value;
        }

        static string FindTimesCsv()
        {
            var path = Environment.CurrentDirectory;

            int sanityCheck = 5;
            while (!File.Exists(Path.Combine(path, "Times.csv")))
            {
                path = Path.Combine(path, "..");
                sanityCheck--;

                if (sanityCheck == 0)
                    throw new Exception("Could not find Times.csv");
            }

            return Path.Combine(Path.GetFullPath(path), "Times.csv");
        }

        return 0;
    }
}