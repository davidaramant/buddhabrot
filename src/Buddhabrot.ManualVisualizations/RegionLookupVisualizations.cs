using Buddhabrot.Core.Boundary;
using Buddhabrot.Core.Boundary.Visualizations;

namespace Buddhabrot.ManualVisualizations;

[Explicit]
public class RegionLookupVisualizations : BaseVisualization
{
    [OneTimeSetUp]
    public void SetOutputPath() => base.SetUpOutputPath(nameof(RegionLookupVisualizations));

    [Test]
    public void ShouldConstructPower2QuadTreeCorrectly()
    {
        var power2Regions =
            new[] {(0, 0), (1, 0), (2, 0), (2, 1), (3, 1), (3, 2), (4, 2), (4, 1), (4, 0)}
                .Select(t => (new RegionId(t.Item1, t.Item2), RegionType.Border)).ToList();

        var lookup = new RegionLookup(new AreaDivisions(2), power2Regions);

        using var image = BoundaryVisualizer.RenderRegionLookup(lookup);
        SaveImage(image, "Power 2 Quadtree");
    }

    [Test]
    public void ShouldConstructPower3QuadTreeCorrectly()
    {
        var power2Regions =
            new[]
                {
                    (0, 0), (1, 0), (2, 0), (3, 0), (3, 1), (4, 1), (4, 0), (5, 0), (5, 1), (5, 2), (6, 2), (7, 2),
                    (8, 2),
                    (9, 2), (9, 1), (9, 0)
                }
                .Select(t => (new RegionId(t.Item1, t.Item2), RegionType.Border)).ToList();

        var lookup = new RegionLookup(new AreaDivisions(3), power2Regions);

        using var image = BoundaryVisualizer.RenderRegionLookup(lookup);
        SaveImage(image, "Power 3 Quadtree");
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
    public void ShouldRenderSimpleQuadTreeWithBorder(int verticalPower, int x, int y)
    {
        var divisions = new AreaDivisions(verticalPower);
        var region = new RegionId(x, y);
        var lookup = new RegionLookup(divisions, new[] {(region, RegionType.Border)});
        using var image = BoundaryVisualizer.RenderRegionLookup(lookup);
        var width = divisions.QuadrantDivisions * 2;
        SaveImage(image, $"Border {width}x{width/2} {region.X} {region.Y}");
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
    public void ShouldRenderSimpleQuadTreeWithFilament(int verticalPower, int x, int y)
    {
        var divisions = new AreaDivisions(verticalPower);
        var region = new RegionId(x, y);
        var lookup = new RegionLookup(divisions, new[] {(region, RegionType.Filament)});
        using var image = BoundaryVisualizer.RenderRegionLookup(lookup);
        var width = divisions.QuadrantDivisions * 2;
        SaveImage(image, $"Filament {width}x{width/2} {region.X} {region.Y}");
    }
}