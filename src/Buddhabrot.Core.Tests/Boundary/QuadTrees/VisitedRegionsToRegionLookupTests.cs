using Buddhabrot.Core.Boundary;
using Buddhabrot.Core.Boundary.QuadTrees;

namespace Buddhabrot.Core.Tests.Boundary.QuadTrees;

public class VisitedRegionsToRegionLookupTests
{
    [Theory]
    [InlineData(LookupRegionType.Border, LookupRegionType.Empty, LookupRegionType.Empty, LookupRegionType.Empty, LookupRegionType.Border)]
    [InlineData(LookupRegionType.Border, LookupRegionType.Filament, LookupRegionType.Filament, LookupRegionType.Empty, LookupRegionType.Filament)]
    public void ShouldCondenseRegionTypes(
        LookupRegionType ll,
        LookupRegionType lr,
        LookupRegionType ul,
        LookupRegionType ur,
        LookupRegionType expected)
    {
        QuadTreeTransformer.CondenseRegionType(ll, lr, ul, ur).Should().Be(expected);
    }

    [Fact]
    public void ShouldCondenseIdenticalLeaves()
    {
        var transformer = new QuadTreeTransformer(new VisitedRegions());
        var node = transformer.MakeQuad(
            RegionNode.Empty,
            RegionNode.Empty,
            RegionNode.Empty,
            RegionNode.Empty);

        node.Should().Be(RegionNode.Empty);
    }
}