using Buddhabrot.Core.Boundary;
using Buddhabrot.Core.Boundary.QuadTrees;

namespace Buddhabrot.Core.Tests.Boundary.QuadTrees;

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

        node.SW.Should().Be(RegionType.Filament);
        node.SE.Should().Be(RegionType.Border);
        node.NW.Should().Be(RegionType.Rejected);
        node.NE.Should().Be(RegionType.Unknown);
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

        node = node.WithQuadrant(Quadrant.SW, RegionType.Border);

        node.NodeType.Should().Be(NodeType.LeafQuad);
    }

    [Fact]
    public void ShouldUpdateChildren()
    {
        var node = QuadNode.UnknownLeaf;

        node = node.WithSW(RegionType.Filament);

        node.SW.Should().Be(RegionType.Filament);
        node.SE.Should().Be(RegionType.Unknown);
        node.NW.Should().Be(RegionType.Unknown);
        node.NE.Should().Be(RegionType.Unknown);

        node = node.WithSE(RegionType.Border);

        node.SW.Should().Be(RegionType.Filament);
        node.SE.Should().Be(RegionType.Border);
        node.NW.Should().Be(RegionType.Unknown);
        node.NE.Should().Be(RegionType.Unknown);

        node = node.WithNW(RegionType.Rejected);

        node.SW.Should().Be(RegionType.Filament);
        node.SE.Should().Be(RegionType.Border);
        node.NW.Should().Be(RegionType.Rejected);
        node.NE.Should().Be(RegionType.Unknown);

        node = node.WithNE(RegionType.Rejected);

        node.SW.Should().Be(RegionType.Filament);
        node.SE.Should().Be(RegionType.Border);
        node.NW.Should().Be(RegionType.Rejected);
        node.NE.Should().Be(RegionType.Rejected);
    }

    [Fact]
    public void ShouldUpdateChildrenWithQuadrant()
    {
        QuadNode.UnknownLeaf.WithQuadrant(Quadrant.SW, RegionType.Border).Should().Be(
            QuadNode.MakeLeaf(RegionType.Unknown,
                RegionType.Border,
                RegionType.Unknown,
                RegionType.Unknown,
                RegionType.Unknown));
        QuadNode.UnknownLeaf.WithQuadrant(Quadrant.SE, RegionType.Border).Should().Be(
            QuadNode.MakeLeaf(RegionType.Unknown,
                RegionType.Unknown,
                RegionType.Border,
                RegionType.Unknown,
                RegionType.Unknown));
        QuadNode.UnknownLeaf.WithQuadrant(Quadrant.NW, RegionType.Border).Should().Be(
            QuadNode.MakeLeaf(RegionType.Unknown,
                RegionType.Unknown,
                RegionType.Unknown,
                RegionType.Border,
                RegionType.Unknown));
        QuadNode.UnknownLeaf.WithQuadrant(Quadrant.NE, RegionType.Border).Should().Be(
            QuadNode.MakeLeaf(RegionType.Unknown,
                RegionType.Unknown,
                RegionType.Unknown,
                RegionType.Unknown,
                RegionType.Border));
    }

    [Theory]
    [InlineData(Quadrant.SW)]
    [InlineData(Quadrant.SE)]
    [InlineData(Quadrant.NW)]
    [InlineData(Quadrant.NE)]
    public void ShouldGetQuadrant(Quadrant quadrant)
    {
        QuadNode.UnknownLeaf
            .WithQuadrant(quadrant, RegionType.Border)
            .GetQuadrant(quadrant)
            .Should().Be(RegionType.Border);
    }
}