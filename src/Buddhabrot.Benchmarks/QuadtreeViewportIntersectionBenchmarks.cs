using BenchmarkDotNet.Attributes;
using Buddhabrot.Core.Boundary;
using SkiaSharp;

namespace Buddhabrot.Benchmarks;

/// <summary>
/// Different ways to compute the intersection of two square boundaries
/// </summary>
public class QuadtreeViewportIntersectionBenchmarks
{
	private const int Size = 100;
	private readonly QuadtreeViewport _boundary = new(
		SKRectI.Create(new SKPointI(x: 10, y: 10), new SKSizeI(100, 100)),
		Scale: 4
	);
	private readonly SKRectI[] _rectangles = new SKRectI[Size];

	[GlobalSetup]
	public void FillArrays()
	{
		var rand = new Random(0);
		for (int i = 0; i < Size; i++)
		{
			_rectangles[i] = SKRectI.Create(rand.Next(20), rand.Next(20), 10, 10);
		}
	}

	[Benchmark(Baseline = true)]
	public int OldMethod()
	{
		int intersections = 0;

		for (int i = 0; i < Size; i++)
		{
			var rect = IntersectWith(_boundary, _rectangles[i]);
			if (rect != SKRectI.Empty)
			{
				intersections++;
			}
		}

		return intersections;

		static SKRectI IntersectWith(QuadtreeViewport boundary, SKRectI rect)
		{
			int length = boundary.Length;

			int x1 = Math.Max(rect.Left, boundary.Area.Left);
			int x2 = Math.Min(rect.Left + rect.Width, boundary.Area.Left + length);
			int y1 = Math.Max(rect.Top, boundary.Area.Top);
			int y2 = Math.Min(rect.Top + rect.Height, boundary.Area.Top + length);

			if (x2 >= x1 && y2 >= y1)
			{
				return new SKRectI(x1, y1, x2, y2);
			}

			return SKRectI.Empty;
		}
	}

	[Benchmark]
	public int DifferentChecks()
	{
		int intersections = 0;

		for (int i = 0; i < Size; i++)
		{
			var rect = IntersectWith(_boundary, _rectangles[i]);
			if (IsValid(rect))
			{
				intersections++;
			}
		}

		return intersections;

		static SKRectI IntersectWith(QuadtreeViewport boundary, SKRectI rect)
		{
			int length = boundary.Length;

			int x1 = Math.Max(rect.Left, boundary.Area.Left);
			int x2 = Math.Min(rect.Left + rect.Width, boundary.Area.Left + length);
			int y1 = Math.Max(rect.Top, boundary.Area.Top);
			int y2 = Math.Min(rect.Top + rect.Height, boundary.Area.Top + length);

			return new SKRectI(x1, y1, x2, y2);
		}

		static bool IsValid(SKRectI rect) => rect is { Width: > 0, Height: > 0 };
	}
}
