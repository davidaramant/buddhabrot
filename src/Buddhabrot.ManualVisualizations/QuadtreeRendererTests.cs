using Buddhabrot.Core.Boundary.Visualization;
using Buddhabrot.Core.Images;
using SkiaSharp;

namespace Buddhabrot.ManualVisualizations;

[Explicit]
public class QuadtreeRendererTests : BaseVisualization
{
	[OneTimeSetUp]
	public void CreateDirectory() => SetUpOutputPath(nameof(QuadtreeRendererTests));

	[Test]
	public void ShouldRenderSingleCellAtDifferentDepths([Range(1, 4)] int levels)
	{
		var width = QuadtreeRenderer.GetRequiredWidth(levels);
		using var image = new RasterImage(width, width);
		foreach (var depth in Enumerable.Range(0, levels))
		{
			image.Fill(SKColors.Black);
			var r = new QuadtreeRenderer(image, levels);
			r.DrawCell(0, 0, depth, SKColors.White);

			SaveImage(image, $"Single Cell - l{levels} d{depth}");
		}
	}

	[Test]
	public void ShouldRenderAllCellsAtDifferentDepths([Range(1, 4)] int levels)
	{
		var width = QuadtreeRenderer.GetRequiredWidth(levels);
		using var image = new RasterImage(width, width);
		foreach (var depth in Enumerable.Range(0, levels))
		{
			image.Fill(SKColors.Black);
			var r = new QuadtreeRenderer(image, levels);

			for (int y = 0; y < (1 << depth); y++)
			{
				for (int x = 0; x < (1 << depth); x++)
				{
					r.DrawCell(x, y, depth, SKColors.White);
				}
			}

			SaveImage(image, $"All Cells - l{levels} d{depth}");
		}
	}

	[Test]
	public void ShouldRenderDiagonalLineOfCells()
	{
		const int levels = 3;
		var width = QuadtreeRenderer.GetRequiredWidth(levels);
		using var image = new RasterImage(width, width);
		image.Fill(SKColors.Black);
		var r = new QuadtreeRenderer(image, levels);

		foreach (var pos in Enumerable.Range(0, 1 << levels - 1))
		{
			r.DrawCell(pos, pos, 2, SKColors.White);
		}

		SaveImage(image, "Diagonal");
	}

	[Test]
	public void ShouldRenderEveryCellOfQuadTree()
	{
		const int levels = 3;

		var width = QuadtreeRenderer.GetRequiredWidth(levels);
		using var image = new RasterImage(width, width);

		for (int y = 0; y < 4; y++)
		{
			for (int x = 0; x < 4; x++)
			{
				image.Fill(SKColors.Black);
				var r = new QuadtreeRenderer(image, levels);
				r.DrawCell(x, y, 2, SKColors.White);
				SaveImage(image, $"Pos Y{y} X{x}");
			}
		}
	}
}
