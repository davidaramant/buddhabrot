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
        var imageWidth = (1 << lookup.Depth) * 2 + 1;
        var nodes = lookup.GetRawNodes();

        using var image = new RasterImage(imageWidth, imageWidth, scale: scale);
        image.Fill(Color.FromArgb(red:25,green:25,blue:25));

        void DrawPixel(int x, int y, Color c) => image.SetPixel(x, imageWidth - y - 1, c);

        void DrawCell(int x, int y, Color c, int level = 0)
        {
            if (level == 0)
            {
                DrawPixel(x * 2 + 1, y * 2 + 1, c);
            }
            else
            {
                var width = 1 << level;

                var startX = x * 2 + 1;
                var startY = y * 2 + 1;

                var endX = (x + width - 1) * 2 + 1;
                var endY = (y + width - 1) * 2 + 1;

                for (int drawY = startY; drawY <= endY; drawY++)
                {
                    for (int drawX = startX; drawX <= endX; drawX++)
                    {
                        DrawPixel(drawX, drawY, c);
                    }
                }
            }
        }

        (Quad SW, Quad NW, Quad NE, Quad SE) GetChildren(Quad quad)
        {
            var index = quad.ChildIndex;
            return (nodes[index], nodes[index + 1], nodes[index + 2], nodes[index + 3]);
        }

        void DrawQuad(Quad quad, int level, int xOffset, int yOffset)
        {
            if (level == 0)
            {
                DrawCell(xOffset, yOffset, PickColorFromType(quad.Type));
                return;
            }

            if (!quad.HasChildren)
            {
                DrawCell(xOffset, yOffset, PickColorFromType(quad.Type), level);
                return;
            }

            var offset = 1 << (level - 1);
            var (sw, nw, ne, se) = GetChildren(quad);
            DrawQuad(sw, level - 1, xOffset, yOffset);
            DrawQuad(nw, level - 1, xOffset, yOffset + offset);
            DrawQuad(ne, level - 1, xOffset + offset, yOffset + offset);
            DrawQuad(se, level - 1, xOffset + offset, yOffset);
        }

        DrawQuad(nodes.Last(), level: lookup.Depth, 0, 0);

        SaveImage(image, name);
    }

    private static Color PickColorFromType(RegionType type) =>
        type switch
        {
            RegionType.Border => Color.DarkBlue,
            RegionType.Filament => Color.Red,
            _ => Color.White,
        };
}