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
        var node = QuadNode.MakeBranch(type, 123_456);

        node.NodeType.Should().Be(NodeType.Branch);
        node.RegionType.Should().Be(type);

        node.ChildIndex.Should().Be(123_456);
    }

    [Fact]
    public void ShouldChangeNodeTypeWhenSettingChildRegion()
    {
        var node = QuadNode.UnknownLeaf;

        node.NodeType.Should().Be(NodeType.Leaf);

        node = node.WithQuadrant(Quadrant.LL, RegionType.Border);

        node.NodeType.Should().Be(NodeType.LeafQuad);
    }

    [Fact]
    public void ShouldUpdateChildren()
    {
        var node = QuadNode.UnknownLeaf;

        node = node.WithLL(RegionType.Filament);

        node.LL.Should().Be(RegionType.Filament);
        node.LR.Should().Be(RegionType.Unknown);
        node.UL.Should().Be(RegionType.Unknown);
        node.UR.Should().Be(RegionType.Unknown);

        node = node.WithLR(RegionType.Border);

        node.LL.Should().Be(RegionType.Filament);
        node.LR.Should().Be(RegionType.Border);
        node.UL.Should().Be(RegionType.Unknown);
        node.UR.Should().Be(RegionType.Unknown);

        node = node.WithUL(RegionType.Rejected);

        node.LL.Should().Be(RegionType.Filament);
        node.LR.Should().Be(RegionType.Border);
        node.UL.Should().Be(RegionType.Rejected);
        node.UR.Should().Be(RegionType.Unknown);

        node = node.WithUR(RegionType.Rejected);

        node.LL.Should().Be(RegionType.Filament);
        node.LR.Should().Be(RegionType.Border);
        node.UL.Should().Be(RegionType.Rejected);
        node.UR.Should().Be(RegionType.Rejected);
    }

    [Fact]
    public void ShouldUpdateChildrenWithQuadrant()
    {
        QuadNode.UnknownLeaf.WithQuadrant(Quadrant.LL, RegionType.Border).Should().Be(
            QuadNode.MakeLeaf(RegionType.Unknown,
                RegionType.Border,
                RegionType.Unknown,
                RegionType.Unknown,
                RegionType.Unknown));
        QuadNode.UnknownLeaf.WithQuadrant(Quadrant.LR, RegionType.Border).Should().Be(
            QuadNode.MakeLeaf(RegionType.Unknown,
                RegionType.Unknown,
                RegionType.Border,
                RegionType.Unknown,
                RegionType.Unknown));
        QuadNode.UnknownLeaf.WithQuadrant(Quadrant.UL, RegionType.Border).Should().Be(
            QuadNode.MakeLeaf(RegionType.Unknown,
                RegionType.Unknown,
                RegionType.Unknown,
                RegionType.Border,
                RegionType.Unknown));
        QuadNode.UnknownLeaf.WithQuadrant(Quadrant.UR, RegionType.Border).Should().Be(
            QuadNode.MakeLeaf(RegionType.Unknown,
                RegionType.Unknown,
                RegionType.Unknown,
                RegionType.Unknown,
                RegionType.Border));
    }

    [Theory]
    [InlineData(Quadrant.LL)]
    [InlineData(Quadrant.LR)]
    [InlineData(Quadrant.UL)]
    [InlineData(Quadrant.UR)]
    public void ShouldGetQuadrant(Quadrant quadrant)
    {
        QuadNode.UnknownLeaf
            .WithQuadrant(quadrant, RegionType.Border)
            .GetQuadrant(quadrant)
            .Should().Be(RegionType.Border);
    }
}