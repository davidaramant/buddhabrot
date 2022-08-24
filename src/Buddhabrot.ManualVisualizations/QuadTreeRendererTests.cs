using System.Drawing;
using Buddhabrot.Core.Boundary;
using Buddhabrot.Core.Boundary.Visualizations;

namespace Buddhabrot.ManualVisualizations;

[Explicit]
public class QuadTreeRendererTests : BaseVisualization
{
    [OneTimeSetUp]
    public void CreateDirectory() => SetUpOutputPath(nameof(QuadTreeRendererTests));

    [Test]
    public void ShouldRenderSingleCellAtDifferentDepths([Range(1, 4)] int levels)
    {
        foreach (var depth in Enumerable.Range(0, levels))
        {
            using var r = new QuadTreeRenderer(levels);
            r.DrawCell(0, 0, depth, Color.White);

            SaveImage(r.Image, $"Single Cell - l{levels} d{depth}");
        }
    }

    [Test]
    public void ShouldRenderAllCellsAtDifferentDepths([Range(1, 4)] int levels)
    {
        foreach (var depth in Enumerable.Range(0, levels))
        {
            using var r = new QuadTreeRenderer(levels);

            for (int y = 0; y < (1 << depth); y++)
            {
                for (int x = 0; x < (1 << depth); x++)
                {
                    r.DrawCell(x, y, depth, Color.White);
                }
            }

            SaveImage(r.Image, $"All Cells - l{levels} d{depth}");
        }
    }

    [Test]
    public void ShouldRenderDiagonalLineOfCells()
    {
        const int levels = 3;
        using var r = new QuadTreeRenderer(levels);

        foreach (var pos in Enumerable.Range(0, 1 << levels))
        {
            r.DrawCell(pos, pos, 2, Color.White);
        }
        
        SaveImage(r.Image, "Diagonal");
    }
    
    [Test]
    public void ShouldRenderEveryCellOfQuadTree()
    {
        const int levels = 3;

        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                using var r = new QuadTreeRenderer(levels);
                r.DrawCell(x, y, 2, Color.White);
                SaveImage(r.Image, $"Pos Y{y} X{x}");
            }
        }
    }
}