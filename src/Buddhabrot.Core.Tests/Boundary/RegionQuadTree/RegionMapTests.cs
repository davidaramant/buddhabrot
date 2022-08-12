using Buddhabrot.Core.Boundary;
using Buddhabrot.Core.Boundary.RegionQuadTree;

namespace Buddhabrot.Core.Tests.Boundary.RegionQuadTree;

public class RegionMapTests
{
    private readonly IReadOnlyList<RegionId> _power2Regions =
        new[] {(0, 0), (1, 0), (2, 0), (2, 1), (3, 1), (3, 2), (4, 2), (4, 1), (4, 0)}
            .Select(t => new RegionId(t.Item1, t.Item2)).ToList();

    private readonly IReadOnlyList<RegionId> _power3Regions =
        new[]
            {
                (0, 0), (1, 0), (2, 0), (3, 0), (3, 1), (4, 1), (4, 0), (5, 0), (5, 1), (5, 2), (6, 2), (7, 2), (8, 2),
                (9, 2), (9, 1), (9, 0)
            }
            .Select(t => new RegionId(t.Item1, t.Item2)).ToList();

    [Fact]
    public void ShouldConstructQuadTreeFromRealBoundaries()
    {
        var _ = new RegionMap(verticalPower: 2, regions: _power2Regions);
        _ = new RegionMap(verticalPower: 3, regions: _power3Regions);
    }

    [Fact]
    public void ShouldReturnEveryAreaIfSearchAreaIsEqualToBounds()
    {
        var regionMap = new RegionMap(2, new[] {new RegionId(0, 0), new RegionId(4, 1)});
        var visibleAreas = regionMap.GetVisibleAreas(
            new ComplexArea(
                new Range(-2, 2),
                new Range(-2, 2)));

        visibleAreas.Should().HaveCount(2);
    }

    [Fact]
    public void ShouldReturnIntersectionsWithSearchArea()
    {
        var regionMap = new RegionMap(2, new[] {new RegionId(0, 0), new RegionId(4, 1)});
        var visibleAreas = regionMap.GetVisibleAreas(
            new ComplexArea(
                new Range(-0.5, 0.5),
                new Range(0, 1)));

        visibleAreas.Should().HaveCount(1);
    }
}