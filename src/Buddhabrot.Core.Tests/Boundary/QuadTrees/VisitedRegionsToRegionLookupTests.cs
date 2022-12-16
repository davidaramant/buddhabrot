using Buddhabrot.Core.Boundary;
using Buddhabrot.Core.Boundary.QuadTrees;

namespace Buddhabrot.Core.Tests.Boundary.QuadTrees;

public class VisitedRegionsToRegionLookupTests
{
    [Theory]
    [InlineData(RegionType.Border, RegionType.Empty, RegionType.Empty, RegionType.Empty, RegionType.Border)]
    [InlineData(RegionType.Border, RegionType.Filament, RegionType.Rejected, RegionType.Empty, RegionType.Border)]
    [InlineData(RegionType.Border, RegionType.Filament, RegionType.Filament, RegionType.Empty, RegionType.Filament)]
    public void ShouldCondenseRegionTypes(
        RegionType ll,
        RegionType lr,
        RegionType ul,
        RegionType ur,
        RegionType expected)
    {
        QuadTreeTransformer.CondenseRegionType(ll, lr, ul, ur).Should().Be(expected);
    }

    [Fact]
    public void ShouldCondenseIdenticalLeaves()
    {
        var normalizer = new QuadTreeTransformer(new VisitedRegions());
        var node = normalizer.MakeQuad(
            RegionNode.Empty,
            RegionNode.Empty,
            RegionNode.Empty,
            RegionNode.Empty);

        node.Should().Be(RegionNode.Empty);
    }
}