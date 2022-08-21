using Buddhabrot.Core.Boundary;

namespace Buddhabrot.Core.Tests.Boundary;

public class RegionLookupTests
{
    private readonly IReadOnlyList<(RegionId,RegionType)> _power2Regions =
        new[] {(0, 0), (1, 0), (2, 0), (2, 1), (3, 1), (3, 2), (4, 2), (4, 1), (4, 0)}
            .Select(t => (new RegionId(t.Item1, t.Item2), RegionType.Border)).ToList();

    private readonly IReadOnlyList<(RegionId,RegionType)> _power3Regions =
        new[]
            {
                (0, 0), (1, 0), (2, 0), (3, 0), (3, 1), (4, 1), (4, 0), (5, 0), (5, 1), (5, 2), (6, 2), (7, 2), (8, 2),
                (9, 2), (9, 1), (9, 0)
            }
            .Select(t => (new RegionId(t.Item1, t.Item2), RegionType.Border)).ToList();

    [Fact]
    public void ShouldConstructQuadTreeFromRealBoundaries()
    {
        var _ = new RegionLookup(verticalPower: 2, regions: _power2Regions);
        _ = new RegionLookup(verticalPower: 3, regions: _power3Regions);
    }

    [Fact]
    public void ShouldReturnEveryAreaPlusMirrorsIfSearchAreaIsEqualToBounds()
    {
        var lookup = new RegionLookup(2, new[] {(new RegionId(0, 0), RegionType.Border), (new RegionId(4, 1), RegionType.Border)});
        var visibleAreas = lookup.GetVisibleAreas(
            new ComplexArea(
                new Range(-2, 2),
                new Range(-2, 2)),
            0.00001);
        
        visibleAreas.Should().HaveCount(4);
    }

    [Fact]
    public void ShouldReturnIntersectionsWithSearchArea()
    {
        var lookup = new RegionLookup(2, new[] {(new RegionId(0, 0), RegionType.Border), (new RegionId(4, 1), RegionType.Border)});
        var visibleAreas = lookup.GetVisibleAreas(
            new ComplexArea(
                new Range(-0.5, 0.5),
                new Range(0, 1)),
            0.000001);

        visibleAreas.Should().HaveCount(1);
    }
}