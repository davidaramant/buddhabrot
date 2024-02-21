using Buddhabrot.Core.Boundary;
using Buddhabrot.Core.Boundary.Visualization;
using Buddhabrot.Core.Tests.Boundary;
using Buddhabrot.Core.Tests.Boundary.QuadTrees;

namespace Buddhabrot.ManualVisualizations;

[Explicit]
public class RegionLookupVisualizations : BaseVisualization
{
    [OneTimeSetUp]
    public void SetOutputPath() => base.SetUpOutputPath(nameof(RegionLookupVisualizations));

    [Test]
    public void ShouldConstructPower2QuadTreeCorrectly()
    {
        var lookup = RegionLookupUtil.Power2Lookup;

        using var image = BoundaryVisualizer.RenderRegionLookup(lookup);
        SaveImage(image, "Power 2 Quadtree");
    }

    [Test]
    public void ShouldConstructPower3QuadTreeCorrectly()
    {
        var lookup = RegionLookupUtil.Power3Lookup;

        using var image = BoundaryVisualizer.RenderRegionLookup(lookup);
        SaveImage(image, "Power 3 Quadtree");
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
    public void ShouldRenderSimpleQuadTreeWithBorder(int x, int y)
    {
        var lookup = RegionLookupUtil.Make((x, y));
        using var image = BoundaryVisualizer.RenderRegionLookup(lookup);
        SaveImage(image, $"Border {x} {y}");
    }
}
