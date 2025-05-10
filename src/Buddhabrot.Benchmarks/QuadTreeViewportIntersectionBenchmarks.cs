using System.Drawing;
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
	private readonly Rectangle[] _rectangles = new Rectangle[Size];

	[GlobalSetup]
	public void FillArrays()
	{
		var rand = new Random(0);
		for (int i = 0; i < Size; i++)
		{
			_rectangles[i] = new Rectangle(rand.Next(20), rand.Next(20), 10, 10);
		}
	}

	[Benchmark(Baseline = true)]
	public int OldMethod()
	{
		int intersections = 0;

		for (int i = 0; i < Size; i++)
		{
			var rect = IntersectWith(_boundary, _rectangles[i]);
			if (rect != Rectangle.Empty)
			{
				intersections++;
			}
		}

		return intersections;

		static Rectangle IntersectWith(QuadTreeViewport boundary, Rectangle rect)
		{
			int length = boundary.Length;

			int x1 = Math.Max(rect.X, boundary.TopLeft.X);
			int x2 = Math.Min(rect.X + rect.Width, boundary.TopLeft.X + length);
			int y1 = Math.Max(rect.Y, boundary.TopLeft.Y);
			int y2 = Math.Min(rect.Y + rect.Height, boundary.TopLeft.Y + length);

			if (x2 >= x1 && y2 >= y1)
			{
				return new Rectangle(x1, y1, x2 - x1, y2 - y1);
			}

			return Rectangle.Empty;
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

		static Rectangle IntersectWith(QuadTreeViewport boundary, Rectangle rect)
		{
			int length = boundary.Length;

			int x1 = Math.Max(rect.X, boundary.TopLeft.X);
			int x2 = Math.Min(rect.X + rect.Width, boundary.TopLeft.X + length);
			int y1 = Math.Max(rect.Y, boundary.TopLeft.Y);
			int y2 = Math.Min(rect.Y + rect.Height, boundary.TopLeft.Y + length);

			return new Rectangle(x1, y1, x2 - x1, y2 - y1);
		}

		static bool IsValid(Rectangle rect) => rect.Width > 0 && rect.Height > 0;
	}
}
