using System.Drawing;
using Buddhabrot.Core;
using Buddhabrot.Core.Calculations;
using Buddhabrot.Core.Images;

namespace Buddhabrot.ManualVisualizations;

[Explicit]
public class FilamentVisualizations : BaseVisualization
{
	readonly ViewPort _viewPort = ViewPort.FromLogicalArea(
		new ComplexArea(new Interval(-2, 2), new Interval(0, 2)),
		width: 1000
	);

	[OneTimeSetUp]
	public void CreateOutputPath() => SetUpOutputPath(nameof(FilamentVisualizations));

	[Test]
	public void ShouldCalculateFilaments()
	{
		var escapeLimitsThousands = new[] { 1, 5, 10 };

		using var image = new RasterImage(_viewPort.Resolution);

		foreach (var maxK in escapeLimitsThousands)
		{
			RenderEscapeTimeSet(_viewPort, maxK * 1000, image);

			SaveImage(image, $"Escape Time {maxK}K");

			RenderFilaments(_viewPort, maxK * 1000, image);

			SaveImage(image, $"Distance Estimate {maxK}K");
		}
	}

	[Test]
	public void ShouldBeAbleToBoundaryTraceWithFilaments([Values] bool checkDiagonals)
	{
		var max = 10_000;

		using var image = new RasterImage(_viewPort.Resolution);

		image.Fill(Color.White);

		var visitedPoints = new HashSet<Point>();
		var toCheck = new Queue<Point>();
		toCheck.Enqueue(new Point(0, _viewPort.Resolution.Height - 1));

		while (toCheck.Any())
		{
			var point = toCheck.Dequeue();

			if (visitedPoints.Contains(point))
				continue;

			var c = _viewPort.GetComplex(point);
			var (_, distance) = ScalarKernel.FindExteriorDistance(c, max);
			// ReSharper disable once CompareOfFloatsByEqualityOperator
			var containsBorderOrFilament = distance == Double.MaxValue || distance < _viewPort.PixelWidth / 2;

			visitedPoints.Add(point);
			image.SetPixel(point, PickColorFromDistance(_viewPort, distance));

			if (containsBorderOrFilament)
			{
				AddPointToCheck(point with { X = point.X + 1 });
				AddPointToCheck(point with { X = point.X - 1 });
				AddPointToCheck(point with { Y = point.Y + 1 });
				AddPointToCheck(point with { Y = point.Y - 1 });

				if (checkDiagonals)
				{
					AddPointToCheck(point with { X = point.X + 1, Y = point.Y + 1 });
					AddPointToCheck(point with { X = point.X + 1, Y = point.Y - 1 });
					AddPointToCheck(point with { X = point.X - 1, Y = point.Y + 1 });
					AddPointToCheck(point with { X = point.X - 1, Y = point.Y - 1 });
				}
			}
		}

		void AddPointToCheck(Point p)
		{
			if (
				p.X >= 0
				&& p.Y >= 0
				&& p.X < _viewPort.Resolution.Width
				&& p.Y < _viewPort.Resolution.Height
				&& !visitedPoints.Contains(p)
			)
			{
				toCheck.Enqueue(p);
			}
		}

		SaveImage(image, $"Boundary Traced Filaments {max / 1000}K - diagonals {(checkDiagonals ? "YES" : "NO")}");
	}

	private static void RenderEscapeTimeSet(ViewPort viewPort, int max, RasterImage image)
	{
		Parallel.For(
			0,
			viewPort.Resolution.Height,
			row =>
			{
				for (int col = 0; col < viewPort.Resolution.Width; col++)
				{
					var inSet = ScalarKernel.FindEscapeTime(viewPort.GetComplex(col, row), max).IsInfinite;

					image.SetPixel(col, row, inSet ? Color.DarkBlue : Color.White);
				}
			}
		);
	}

	private static void RenderFilaments(ViewPort viewPort, int max, RasterImage image)
	{
		Parallel.For(
			0,
			viewPort.Resolution.Height,
			row =>
			{
				for (int col = 0; col < viewPort.Resolution.Width; col++)
				{
					var (_, distance) = ScalarKernel.FindExteriorDistance(viewPort.GetComplex(col, row), max);

					image.SetPixel(col, row, PickColorFromDistance(viewPort, distance));
				}
			}
		);
	}

	private static Color PickColorFromDistance(ViewPort viewPort, double distance) =>
		distance switch
		{
			Double.MaxValue => Color.DarkBlue,
			var d when d < viewPort.PixelWidth / 2 => Color.Red,
			_ => Color.White,
		};
}
