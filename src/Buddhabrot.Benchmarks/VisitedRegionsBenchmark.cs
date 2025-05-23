using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Buddhabrot.Core.Boundary;
using Buddhabrot.Core.Tests.Boundary;

namespace Buddhabrot.Benchmarks;

/// <summary>
/// Compares the various ways to keep track of the visited regions.
/// </summary>
[SimpleJob(RunStrategy.Monitoring, warmupCount: 1, iterationCount: 3)]
[MemoryDiagnoser]
public class VisitedRegionsBenchmark
{
	private VisitedRegionsDataSet.SavedData _savedData = default!;

	[GlobalSetup]
	public void LoadDataSet() => _savedData = VisitedRegionsDataSet.Load();

	[Benchmark]
	[ArgumentsSource(nameof(AllVisitedRegionsImplementations))]
	public void UseVisitedRegions(DescribedImplementation wrapper)
	{
		var regions = wrapper.CreateRegions();
		for (int i = 0; i < _savedData.Metadata.Count; i++)
		{
			if (_savedData.GetCommandType(i) == VisitedRegionsDataSet.CommandType.Add)
			{
				regions.Visit(_savedData.GetId(i), _savedData.GetRegionType(i));
			}
			else
			{
				regions.HasVisited(_savedData.GetId(i));
			}
		}
	}

	public sealed record DescribedImplementation(Func<IVisitedRegions> CreateRegions, string Description)
	{
		public override string ToString() => Description;
	}

	public IEnumerable<object> AllVisitedRegionsImplementations()
	{
		yield return new DescribedImplementation(() => new HashSetVisitedRegions(), "HashSet");
		yield return new DescribedImplementation(
			() => new HashSetListVisitedRegions(VisitedRegionsDataSet.Size.QuadrantDivisions),
			"List of HashSets"
		);
		yield return new DescribedImplementation(
			() => new VisitedRegions(VisitedRegionsDataSet.Size.QuadrantDivisions * 2),
			"Quad Tree"
		);
	}

	public sealed class HashSetVisitedRegions : IVisitedRegions
	{
		private readonly HashSet<RegionId> _cache = new();

		public bool Visit(RegionId id, VisitedRegionType _) => _cache.Add(id);

		public bool HasVisited(RegionId id) => _cache.Contains(id);

		public override string ToString() => "HashSet Visited Regions";
	}
}
