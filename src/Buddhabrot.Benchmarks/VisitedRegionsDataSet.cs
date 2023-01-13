using Buddhabrot.Core.Boundary;
using Buddhabrot.Core.Boundary.Corners;
using Buddhabrot.ManualVisualizations;
using Humanizer;
using ProtoBuf;
using Spectre.Console;

namespace Buddhabrot.Benchmarks;

public static class VisitedRegionsDataSet
{
    public static readonly AreaDivisions Size = new(16);

    private static readonly string DataFilePath =
        Path.Combine(
            DataLocation.CreateDirectory("Benchmarks", nameof(VisitedRegionsDataSet).Humanize()).FullName,
            "VisitedRegions.bin");

    public static void Create()
    {
        if (!File.Exists(DataFilePath))
        {
            AnsiConsole.Progress()
                .Columns(
                    new TaskDescriptionColumn(),
                    new ProgressBarColumn(),
                    new ElapsedTimeColumn()
                )
                .Start(ctx =>
                {
                    var boundaryTask = ctx.AddTask("Finding visited regions");
                    boundaryTask.IsIndeterminate = true;

                    var bp = new BoundaryParameters(Size, 1_000_000);

                    var proxy = new VisitedRegionsProxy(bp.Divisions.QuadrantDivisions);
                    BoundaryCalculator.VisitBoundary(
                        new RegionInspector(bp),
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

    public static SavedData Load()
    {
        Console.Out.WriteLine("Loading data set...");

        using var fs = File.OpenRead(DataFilePath);
        var sd = Serializer.Deserialize<SavedData>(fs);

        Console.Out.WriteLine($"Loaded {sd.Metadata.Count:N0} commands");

        return sd;
    }

    private sealed class VisitedRegionsProxy : IVisitedRegions
    {
        private readonly VisitedRegions _visitedRegions;

        public readonly SavedData Commands = new();

        public VisitedRegionsProxy(int rows) => _visitedRegions = new VisitedRegions(rows);

        public void Visit(RegionId id, VisitedRegionType type)
        {
            Commands.LogAdd(id, type);
            _visitedRegions.Visit(id, type);
        }

        public bool HasVisited(RegionId id)
        {
            Commands.LogContains(id);
            return _visitedRegions.HasVisited(id);
        }
    }

    #region Data Format

    [ProtoContract]
    public sealed class SavedData
    {
        [ProtoMember(1)] public List<byte> Metadata { get; set; } = new();
        [ProtoMember(2)] public List<int> X { get; set; } = new();
        [ProtoMember(3)] public List<int> Y { get; set; } = new();

        public int Count => Metadata.Count;

        public CommandType GetCommandType(int i) => (CommandType)(Metadata[i] & 1);

        public VisitedRegionType GetRegionType(int i) => (VisitedRegionType)(Metadata[i] >> 1);

        public RegionId GetId(int i) => new(X[i], Y[i]);

        public void LogAdd(RegionId id, VisitedRegionType type)
        {
            Metadata.Add((byte)((byte)type << 1));
            X.Add(id.X);
            Y.Add(id.Y);
        }

        public void LogContains(RegionId id)
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
}