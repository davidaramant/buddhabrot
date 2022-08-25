using Buddhabrot.Core.Boundary;
using Buddhabrot.Core.Boundary.Visualizations;

namespace Buddhabrot.ManualVisualizations;

[Explicit]
public class RegionLookupVisualizations : BaseVisualization
{
    [OneTimeSetUp]
    public void SetOutputPath() => base.SetUpOutputPath(nameof(RegionLookupVisualizations));

    [Test]
    public void ShouldConstructQuadTreeCorrectly()
    {
        var power2Regions =
            new[] { (0, 0), (1, 0), (2, 0), (2, 1), (3, 1), (3, 2), (4, 2), (4, 1), (4, 0) }
                .Select(t => (new RegionId(t.Item1, t.Item2), RegionType.Border)).ToList();

        var lookup = new RegionLookup(new AreaDivisions(2), power2Regions);

        using var image = BoundaryVisualizer.RenderRegionLookup(lookup, scale: 10);
        SaveImage(image, "Power 2 Quadtree");
    }

    [Test]
    [TestCase(1, 0, 0)]
    [TestCase(1, 0, 1)]
    [TestCase(1, 1, 1)]
    [TestCase(1, 1, 0)]
    [TestCase(1, 2, 0)]
    [TestCase(1, 2, 1)]
    [TestCase(1, 3, 1)]
    [TestCase(1, 3, 0)]
    [TestCase(2, 0, 0)]
    [TestCase(2, 1, 1)]
    [TestCase(2, 2, 2)]
    [TestCase(2, 3, 3)]
    [TestCase(2, 4, 0)]
    [TestCase(2, 4, 1)]
    [TestCase(2, 5, 1)]
    [TestCase(2, 7, 3)]
    public void ShouldRenderSimpleQuadTree(int verticalPower, int x, int y)
    {
        var divisions = new AreaDivisions(verticalPower);
        var region = new RegionId(x, y);
        var lookup = new RegionLookup(divisions, new[] { (region, RegionType.Border) });
        using var image = BoundaryVisualizer.RenderRegionLookup(lookup);
        var width = divisions.QuadrantDivisions * 2;
        SaveImage(image, $"{width}x{width} {region.X} {region.Y}");
    }
}