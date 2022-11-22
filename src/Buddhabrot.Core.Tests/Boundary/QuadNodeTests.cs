using Buddhabrot.Core.Boundary;

namespace Buddhabrot.Core.Tests.Boundary;

public class QuadNodeTests
{
    [Theory]
    [InlineData(RegionType.Unknown)]
    [InlineData(RegionType.Border)]
    [InlineData(RegionType.Filament)]
    [InlineData(RegionType.Rejected)]
    public void ShouldConstructLeafCorrectly(RegionType type)
    {
        var node = QuadNode.MakeLeaf(type);

        node.NodeType.Should().Be(NodeType.Leaf);
        node.RegionType.Should().Be(type);
    }

    [Theory]
    [InlineData(RegionType.Unknown)]
    [InlineData(RegionType.Border)]
    [InlineData(RegionType.Filament)]
    [InlineData(RegionType.Rejected)]
    public void ShouldConstructLeafQuadCorrectly(RegionType type)
    {
        var node = QuadNode.MakeLeaf(
            type,
            RegionType.Filament,
            RegionType.Border,
            RegionType.Rejected,
            RegionType.Unknown);
        
        node.NodeType.Should().Be(NodeType.LeafQuad);
        node.RegionType.Should().Be(type);

        node.LL.Should().Be(RegionType.Filament);
        node.LR.Should().Be(RegionType.Border);
        node.UL.Should().Be(RegionType.Rejected);
        node.UR.Should().Be(RegionType.Unknown);
    }
    
    [Theory]
    [InlineData(RegionType.Unknown)]
    [InlineData(RegionType.Border)]
    [InlineData(RegionType.Filament)]
    [InlineData(RegionType.Rejected)]
    public void ShouldConstructBranchCorrectly(RegionType type)
    {
        var node = QuadNode.MakeBranch(type,123_456);

        node.NodeType.Should().Be(NodeType.Branch);
        node.RegionType.Should().Be(type);

        node.ChildIndex.Should().Be(123_456);
    }
}