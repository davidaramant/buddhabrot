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
[MarkdownExporterAttribute.GitHub]
[MemoryDiagnoser]
public class VisitedRegionsBenchmark
{
    private VisitedRegionsDataSet.SavedData _savedData = default!;

    [GlobalSetup]
    public void LoadDataSet()
    {
        Console.Out.WriteLine("Loading data set...");

        _savedData = VisitedRegionsDataSet.Load();

        Console.Out.WriteLine($"Loaded {_savedData.Metadata.Count:N0} commands");
    }

    [Benchmark]
    [ArgumentsSource(nameof(AllVisitedRegionsImplementations))]
    public void UseVisitedRegions(DescribedImplementation wrapper)
    {
        var regions = wrapper.Regions;
        for (int i = 0; i < _savedData.Metadata.Count; i++)
        {
            if (_savedData.CommandType(i) == VisitedRegionsDataSet.CommandType.Add)
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
            new HashSetListVisitedRegions(VisitedRegionsDataSet.Size.QuadrantDivisions),
            "List of HashSets");
        yield return new DescribedImplementation(
            new VisitedRegions(VisitedRegionsDataSet.Size.QuadrantDivisions * 2),
            "Quad Tree");
    }
    
    public sealed class HashSetVisitedRegions : IVisitedRegions
    {
        private readonly HashSet<RegionId> _cache = new();

        public void Visit(RegionId id, RegionType _) => _cache.Add(id);

        public bool HasVisited(RegionId id) => _cache.Contains(id);

        public override string ToString() => "HashSet Visited Regions";
    }
}