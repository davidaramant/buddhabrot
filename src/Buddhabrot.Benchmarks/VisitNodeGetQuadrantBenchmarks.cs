using BenchmarkDotNet.Attributes;
using Buddhabrot.Core.Boundary;
using Buddhabrot.Core.Boundary.QuadTrees;

namespace Buddhabrot.Benchmarks;

public class VisitNodeGetQuadrantBenchmarks
{
	private const int Size = 100;
	private readonly Quadrant[] _quadrants = new Quadrant[Size];

	[GlobalSetup]
	public void FillArrays()
	{
		var rand = new Random(0);
		for (int i = 0; i < Size; i++)
		{
			_quadrants[i] = (Quadrant)rand.Next(4);
		}
	}

	[Benchmark(Baseline = true)]
	public VisitedRegionType Switch()
	{
		var node = VisitNode.Unknown;

		VisitedRegionType type = default;
		for (int i = 0; i < Size; i++)
		{
			type = GetQuadrant(node, _quadrants[i]);
		}

		return type;

		static VisitedRegionType GetQuadrant(VisitNode node, Quadrant quadrant) =>
			quadrant switch
			{
				Quadrant.SW => node.SW,
				Quadrant.SE => node.SE,
				Quadrant.NW => node.NW,
				Quadrant.NE => node.NE,
				_ => throw new Exception("Can't happen")
			};
	}

	[Benchmark]
	public VisitedRegionType Branchless()
	{
		var node = VisitNode.Unknown;

		VisitedRegionType type = default;
		for (int i = 0; i < Size; i++)
		{
			type = GetQuadrant(node, _quadrants[i]);
		}

		return type;

		static VisitedRegionType GetQuadrant(VisitNode node, Quadrant quadrant)
		{
			var offset = QuadrantOffset(quadrant);
			return (VisitedRegionType)(node.Encoded >> offset & 0b11);
		}
	}

	private static int QuadrantOffset(Quadrant quadrant) => 10 - 2 * (int)quadrant;
}
