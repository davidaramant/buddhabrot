using System.Drawing;
using Buddhabrot.Core.Boundary;
using Buddhabrot.Core.Images;

namespace Buddhabrot.ManualVisualizations;

[Explicit]
public class QuadTreeVisualizations : BaseVisualization
{
    [OneTimeSetUp]
    public void SetOutputPath() => base.SetUpOutputPath(nameof(QuadTreeVisualizations));

    [Test]
    public void ShouldConstructQuadTreeCorrectly()
    {
        var power2Regions =
            new[] { (0, 0), (1, 0), (2, 0), (2, 1), (3, 1), (3, 2), (4, 2), (4, 1), (4, 0) }
                .Select(t => (new RegionId(t.Item1, t.Item2), RegionType.Border)).ToList();

        var lookup = new RegionLookup(verticalPower: 2, power2Regions);

        RenderQuadTree(lookup, "Power 2 Quadtree", scale: 20);
    }

    [Test]
    [TestCase(0, 0)]
    [TestCase(0, 1)]
    [TestCase(1, 1)]
    [TestCase(1, 0)]
    [TestCase(2, 0)]
    [TestCase(2, 1)]
    [TestCase(3, 1)]
    [TestCase(3, 0)]
    public void ShouldRenderSimplestPossibleQuadTree(int x, int y)
    {
        var region = new RegionId(x, y);
        var lookup = new RegionLookup(verticalPower: 1, new[] { (region, RegionType.Border) });
        RenderQuadTree(lookup, $"Test {region.X} {region.Y}", scale: 20);
    }

    private void RenderQuadTree(RegionLookup lookup, string name, int scale = 1)
    {
        using var r = new QuadTreeRenderer(lookup.Depth);

        var nodes = lookup.GetRawNodes();

        (Quad SW, Quad NW, Quad NE, Quad SE) GetChildren(Quad quad)
        {
            var index = quad.ChildIndex;
            return (nodes[index], nodes[index + 1], nodes[index + 2], nodes[index + 3]);
        }

        void DrawQuad(Quad quad, int depth, int x, int y)
        {
            if (depth == lookup.Depth - 1 || !quad.HasChildren)
            {
                r.DrawCell(x, y, depth, PickColorFromType(quad.Type));
                return;
            }

            var multiplier = 1 << depth;
            var newX = x * multiplier;
            var newY = y * multiplier;
            var (sw, nw, ne, se) = GetChildren(quad);
            DrawQuad(sw, depth + 1, newX, newY);
            DrawQuad(nw, depth + 1, newX, newY + 1);
            DrawQuad(ne, depth + 1, newX + 1, newY + 1);
            DrawQuad(se, depth + 1, newX + 1, newY);
        }

        DrawQuad(nodes.Last(), depth: 0, 0, 0);

        SaveImage(r.Image, name);
    }


    private static Color PickColorFromType(RegionType type) =>
        type switch
        {
            RegionType.Border => Color.DarkBlue,
            RegionType.Filament => Color.Red,
            _ => Color.White,
        };
}