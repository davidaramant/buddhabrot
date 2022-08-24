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
            new[] {(0, 0), (1, 0), (2, 0), (2, 1), (3, 1), (3, 2), (4, 2), (4, 1), (4, 0)}
                .Select(t => (new RegionId(t.Item1, t.Item2), RegionType.Border)).ToList();

        var lookup = new RegionLookup(verticalPower: 2, power2Regions);

        using var image = BoundaryVisualizer.RenderRegionLookup(lookup, scale: 10);
        SaveImage(image, "Power 2 Quadtree");
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
        var lookup = new RegionLookup(verticalPower: 1, new[] {(region, RegionType.Border)});
        using var image = BoundaryVisualizer.RenderRegionLookup(lookup, scale: 10);
        SaveImage(image, $"Test {region.X} {region.Y}");
    }
}