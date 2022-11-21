using System.Text.Json;
using BenchmarkDotNet.Attributes;
using Buddhabrot.Core.Boundary;
using Buddhabrot.ManualVisualizations;
using Humanizer;
using Spectre.Console;

namespace Buddhabrot.Benchmarks;

internal class VisitedRegionsBenchmark
{
    private static readonly string DataFilePath =
        Path.Combine(
            DataLocation.CreateDirectory("Benchmarks", nameof(VisitedRegionsBenchmark).Humanize()).FullName,
            "VisitedPoints.json");

    [GlobalSetup]
    public void LoadDataSet()
    {

    }

    public static void CreateDataSet()
    {
        if (!File.Exists(DataFilePath))
        {
            AnsiConsole.Status().Start("Creating Data Set...", ctx =>
            {
                AnsiConsole.MarkupLine("Finding visited regions...");
                var proxy = new VisitedRegionsProxy();
                BoundaryCalculator.FindBoundaryAndFilaments(
                    new BoundaryParameters(new AreaDivisions(16), 5_000_000),
                    log: _ => { },
                    visitedRegionsArg: proxy);

                AnsiConsole.MarkupLine("Saving data set to file...");
                using var fs = File.Open(DataFilePath, FileMode.Create);
                JsonSerializer.Serialize(fs, proxy.Actions);
            });
        }
    }

    private sealed class VisitedRegionsProxy : IVisitedRegions
    {
        private readonly VisitedRegions _visitedRegions = new();
        public enum ActionType
        {
            Add,
            Contains
        }
        public List<(ActionType, RegionId, RegionType)> Actions { get; } = new List<(ActionType, RegionId, RegionType)>();


        public int Count => _visitedRegions.Count;

        public void Add(RegionId id, RegionType type)
        {
            Actions.Add((ActionType.Add, id, type));
            _visitedRegions.Add(id, type);
        }

        public bool Contains(RegionId id)
        {
            Actions.Add((ActionType.Contains, id, default(RegionType)));
            return _visitedRegions.Contains(id);
        }
    }
}
