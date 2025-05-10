using BenchmarkDotNet.Attributes;
using Buddhabrot.Core.Boundary;
using SkiaSharp;

namespace Buddhabrot.Benchmarks;

/// <summary>
/// Different ways to compute the intersection of two square boundaries
/// </summary>
public class QuadTreeViewportIntersectionBenchmarks
{
	private const int Size = 100;
	private readonly QuadTreeViewport _boundary = new(new SKPointI(x: 10, y: 10), Scale: 4);
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

		static SKRectI IntersectWith(QuadTreeViewport boundary, SKRectI rect)
		{
			int length = boundary.Length;

			int x1 = Math.Max(rect.Left, boundary.TopLeft.X);
			int x2 = Math.Min(rect.Left + rect.Width, boundary.TopLeft.X + length);
			int y1 = Math.Max(rect.Top, boundary.TopLeft.Y);
			int y2 = Math.Min(rect.Top + rect.Height, boundary.TopLeft.Y + length);

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

		static SKRectI IntersectWith(QuadTreeViewport boundary, SKRectI rect)
		{
			int length = boundary.Length;

			int x1 = Math.Max(rect.Left, boundary.TopLeft.X);
			int x2 = Math.Min(rect.Left + rect.Width, boundary.TopLeft.X + length);
			int y1 = Math.Max(rect.Top, boundary.TopLeft.Y);
			int y2 = Math.Min(rect.Top + rect.Height, boundary.TopLeft.Y + length);

			return new SKRectI(x1, y1, x2, y2);
		}

		static bool IsValid(SKRectI rect) => rect is { Width: > 0, Height: > 0 };
	}
}
