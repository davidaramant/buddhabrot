using System.Drawing;

namespace Buddhabrot.ManualVisualizations;

[Explicit]
public class QuadTreeRendererTests : BaseVisualization
{
    [OneTimeSetUp]
    public void CreateDirectory() => SetUpOutputPath(nameof(QuadTreeRendererTests));

    [Test]
    public void ShouldRenderSingleCellAtDifferentDepths([Range(1, 4)] int maxDepth)
    {
        foreach (var depth in Enumerable.Range(0, maxDepth))
        {
            using var r = new QuadTreeRenderer(maxDepth);
            r.DrawCell(0, 0, depth, Color.White);

            SaveImage(r.Image, $"Single Cell - m{maxDepth} d{depth}");
        }
    }

    [Test]
    public void ShouldRenderAllCellsAtDifferentDepths([Range(1, 4)] int maxDepth)
    {
        foreach (var depth in Enumerable.Range(0, maxDepth))
        {
            using var r = new QuadTreeRenderer(maxDepth);

            for (int y = 0; y < (1 << depth); y++)
            {
                for (int x = 0; x < (1 << depth); x++)
                {
                    r.DrawCell(x, y, depth, Color.White);
                }
            }

            SaveImage(r.Image, $"All Cells - m{maxDepth} d{depth}");
        }
    }

    [Test]
    public void ShouldRenderDiagonalLineOfCells()
    {
        const int maxDepth = 3;
        using var r = new QuadTreeRenderer(maxDepth);

        foreach (var pos in Enumerable.Range(0, 1 << maxDepth))
        {
            r.DrawCell(pos, pos, 2, Color.White);
        }
        
        SaveImage(r.Image, "Diagonal");
    }
}