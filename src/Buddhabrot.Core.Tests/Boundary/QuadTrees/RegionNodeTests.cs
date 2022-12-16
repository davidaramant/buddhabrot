using Buddhabrot.Core.Boundary;
using Buddhabrot.Core.Boundary.QuadTrees;

namespace Buddhabrot.Core.Tests.Boundary.QuadTrees;

public class RegionNodeTests
{
    [Theory]
    [InlineData(RegionType.Empty)]
    [InlineData(RegionType.Border)]
    [InlineData(RegionType.Filament)]
    [InlineData(RegionType.Rejected)]
    public void ShouldConstructLeafCorrectly(RegionType type)
    {
        var node = RegionNode.MakeLeaf(type);

        node.IsLeaf.Should().BeTrue();
        node.RegionType.Should().Be(type);
    }

    [Theory]
    [InlineData(RegionType.Empty)]
    [InlineData(RegionType.Border)]
    [InlineData(RegionType.Filament)]
    [InlineData(RegionType.Rejected)]
    public void ShouldConstructBranchCorrectly(RegionType type)
    {
        var node = RegionNode.MakeBranch(type, 123_456);

        node.IsLeaf.Should().BeFalse();
        node.RegionType.Should().Be(type);

        node.ChildIndex.Should().Be(123_456);
    }
}