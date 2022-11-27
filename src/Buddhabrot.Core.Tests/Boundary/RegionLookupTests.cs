using System.Drawing;
using Buddhabrot.Core.Boundary;

namespace Buddhabrot.Core.Tests.Boundary;

public class RegionLookupTests
{
    [Fact]
    public void ShouldReturnEveryAreaPlusMirrorsIfSearchAreaIsEqualToBounds()
    {
        var lookup = RegionLookupUtil.Make((0, 0), (4, 1));
        var visibleAreas = lookup.GetVisibleAreas(
            new SquareBoundary(0, 0, 2),
            new[] {new Rectangle(0, 0, 8, 8)});

        visibleAreas.Should().HaveCount(4);
    }

    [Fact]
    public void ShouldReturnIntersectionsWithSearchArea()
    {
        var lookup = RegionLookupUtil.Make((0, 0), (4, 1));
        var visibleAreas = lookup.GetVisibleAreas(
            new SquareBoundary(0, 0, 2),
            new[] {new Rectangle(0, 0, 1, 1)});

        visibleAreas.Should().HaveCount(1);
    }
}