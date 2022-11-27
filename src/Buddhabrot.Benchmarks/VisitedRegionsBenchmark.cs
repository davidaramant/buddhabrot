using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Buddhabrot.Core.Boundary;
using Buddhabrot.Core.Tests.Boundary;
using Buddhabrot.ManualVisualizations;
using Humanizer;
using ProtoBuf;
using Spectre.Console;

namespace Buddhabrot.Benchmarks;

[SimpleJob(RunStrategy.Monitoring, warmupCount: 1, targetCount: 3)]
public class VisitedRegionsBenchmark
{
    private static readonly string DataFilePath =
        Path.Combine(
            DataLocation.CreateDirectory("Benchmarks", nameof(VisitedRegionsBenchmark).Humanize()).FullName,
            "VisitedPoints.bin");

    private SavedData _savedData = default!;

    [GlobalSetup]
    public void LoadDataSet() =>
        AnsiConsole.Progress()
            .Columns(new ProgressColumn[]
            {
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new ElapsedTimeColumn(),
            })
            .Start(ctx =>
            {
                var loadingTask = ctx.AddTask("Loading visited regions");
                loadingTask.IsIndeterminate = true;

                using var fs = File.OpenRead(DataFilePath);
                _savedData = Serializer.Deserialize<SavedData>(fs);

                loadingTask.StopTask();

                AnsiConsole.MarkupLine($"{_savedData.Metadata.Count:N0} commands");
            });

    [Benchmark]
    [ArgumentsSource(nameof(AllVisitedRegionsImplementations))]
    public void UseVisitedRegions(DescribedImplementation wrapper)
    {
        var regions = wrapper.Regions;
        for (int i = 0; i < _savedData.Metadata.Count; i++)
        {
            if (_savedData.CommandType(i) == CommandType.Add)
            {
                regions.Visit(_savedData.Id(i), _savedData.RegionType(i));
            }
            else
            {
                regions.HasVisited(_savedData.Id(i));
            }
        }
    }

    public sealed record DescribedImplementation(IVisitedRegions Regions, string Description)
    {
        public override string ToString() => Description;
    }

    public IEnumerable<object> AllVisitedRegionsImplementations()
    {
        yield return new DescribedImplementation(
            new HashSetVisitedRegions(),
            "HashSet");
        yield return new DescribedImplementation(
            new HashSetListVisitedRegions(new AreaDivisions(16).QuadrantDivisions),
            "List of HashSets");
        yield return new DescribedImplementation(
            new VisitedRegions(new AreaDivisions(16).QuadrantDivisions * 2),
            "Quad Tree");
    }

    public static void CreateDataSet()
    {
        if (!File.Exists(DataFilePath))
        {
            AnsiConsole.Progress()
                .Columns(new ProgressColumn[]
                {
                    new TaskDescriptionColumn(),
                    new ProgressBarColumn(),
                    new ElapsedTimeColumn(),
                })
                .Start(ctx =>
                {
                    var boundaryTask = ctx.AddTask("Finding visited regions");
                    boundaryTask.IsIndeterminate = true;

                    var bp = new BoundaryParameters(new AreaDivisions(16), 1_000_000);

                    var proxy = new VisitedRegionsProxy(bp.Divisions.QuadrantDivisions);
                    BoundaryCalculator.VisitBoundary(
                        bp,
                        visitedRegions: proxy);

                    boundaryTask.StopTask();

                    AnsiConsole.MarkupLine($"{proxy.Commands.Metadata.Count:N0} commands");

                    var saveTask = ctx.AddTask("Saving data set");
                    saveTask.IsIndeterminate = true;

                    using var fs = File.Open(DataFilePath, FileMode.Create);
                    Serializer.Serialize(fs, proxy.Commands);

                    saveTask.StopTask();
                });
        }
        else
        {
            AnsiConsole.MarkupLine("Data set file already exists");
        }
    }

    #region Data Format

    [ProtoContract]
    public sealed class SavedData
    {
        [ProtoMember(1)] public List<byte> Metadata { get; set; } = new();
        [ProtoMember(2)] public List<int> X { get; set; } = new();
        [ProtoMember(3)] public List<int> Y { get; set; } = new();

        public CommandType CommandType(int i) => (CommandType) (Metadata[i] & 1);

        public RegionType RegionType(int i) => (RegionType) (Metadata[i] >> 1);

        public RegionId Id(int i) => new(X[i], Y[i]);

        public void Add(RegionId id, RegionType type)
        {
            Metadata.Add((byte) ((byte) type << 1));
            X.Add(id.X);
            Y.Add(id.Y);
        }

        public void Contains(RegionId id)
        {
            Metadata.Add(1);
            X.Add(id.X);
            Y.Add(id.Y);
        }
    }

    public enum CommandType
    {
        Add = 0,
        Contains = 1
    }

    #endregion

    private sealed class VisitedRegionsProxy : IVisitedRegions
    {
        private readonly HashSetListVisitedRegions _visitedRegions;

        public readonly SavedData Commands = new();

        public VisitedRegionsProxy(int rows) => _visitedRegions = new HashSetListVisitedRegions(rows);

        public int Count => _visitedRegions.Count;

        public void Visit(RegionId id, RegionType type)
        {
            Commands.Add(id, type);
            _visitedRegions.Visit(id, type);
        }

        public bool HasVisited(RegionId id)
        {
            Commands.Contains(id);
            return _visitedRegions.HasVisited(id);
        }
    }

    public sealed class HashSetVisitedRegions : IVisitedRegions
    {
        private readonly HashSet<RegionId> _cache = new();

        public int Count => _cache.Count;

        public void Visit(RegionId id, RegionType _) => _cache.Add(id);

        public bool HasVisited(RegionId id) => _cache.Contains(id);

        public override string ToString() => "HashSet Visited Regions";
    }
}