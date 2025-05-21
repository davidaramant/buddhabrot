using BenchmarkDotNet.Attributes;
using Buddhabrot.Core.Boundary.Quadtrees;

namespace Buddhabrot.Benchmarks;

public class QuadDimensionGetQuadrantBenchmarks
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
	public QuadDimensions Switch()
	{
		var qd = new QuadDimensions(0, 0, 2);

		QuadDimensions d = default;

		for (int i = 0; i < Size; i++)
		{
			d = GetQuadrant(qd, _quadrants[i]);
		}

		return d;

		static QuadDimensions GetQuadrant(QuadDimensions q, Quadrant quadrant) =>
			quadrant switch
			{
				Quadrant.SW => q.SW,
				Quadrant.SE => q.SE,
				Quadrant.NW => q.NW,
				Quadrant.NE => q.NE,
				_ => throw new Exception("Can't happen"),
			};
	}

	[Benchmark]
	public QuadDimensions Branchless()
	{
		var qd = new QuadDimensions(0, 0, 2);

		QuadDimensions d = default;

		for (int i = 0; i < Size; i++)
		{
			d = GetQuadrant(qd, _quadrants[i]);
		}

		return d;

		static QuadDimensions GetQuadrant(QuadDimensions q, Quadrant quadrant)
		{
			var isLeft = (int)quadrant % 2;
			var isLower = (int)quadrant / 2;

			return new(X: q.X + isLeft * q.QuadrantLength, Y: q.Y + isLower * q.QuadrantLength, Height: q.Height - 1);
		}
	}
}
