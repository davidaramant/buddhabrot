using Buddhabrot.Core.Boundary;
using Buddhabrot.Core.Boundary.QuadTrees;

namespace Buddhabrot.Core.Tests.Boundary.QuadTrees;

public class VisitNodeTests
{
    [Theory]
    [InlineData(VisitedRegionType.Unknown)]
    [InlineData(VisitedRegionType.Border)]
    [InlineData(VisitedRegionType.Filament)]
    [InlineData(VisitedRegionType.Rejected)]
    public void ShouldConstructLeafCorrectly(VisitedRegionType type)
    {
        var node = VisitNode.MakeLeaf(type);

        node.NodeType.Should().Be(VisitNodeType.Leaf);
        node.RegionType.Should().Be(type);
    }

    [Fact]
    public void ShouldConstructLeafQuadCorrectly()
    {
        var node = VisitNode.MakeLeaf(
            VisitedRegionType.Filament,
            VisitedRegionType.Border,
            VisitedRegionType.Rejected,
            VisitedRegionType.Unknown);

        node.NodeType.Should().Be(VisitNodeType.LeafQuad);

        node.SW.Should().Be(VisitedRegionType.Filament);
        node.SE.Should().Be(VisitedRegionType.Border);
        node.NW.Should().Be(VisitedRegionType.Rejected);
        node.NE.Should().Be(VisitedRegionType.Unknown);
    }

    [Fact]
    public void ShouldConstructBranchCorrectly()
    {
        var node = VisitNode.MakeBranch(123_456);

        node.NodeType.Should().Be(VisitNodeType.Branch);

        node.ChildIndex.Should().Be(123_456);
    }

    [Fact]
    public void ShouldChangeNodeTypeWhenSettingChildRegion()
    {
        var node = VisitNode.Unknown;

        node.NodeType.Should().Be(VisitNodeType.Leaf);

        node = node.WithQuadrant(Quadrant.SW, VisitedRegionType.Border);

        node.NodeType.Should().Be(VisitNodeType.LeafQuad);
    }

    [Fact]
    public void ShouldUpdateChildren()
    {
        var node = VisitNode.Unknown;

        node = node.WithSW(VisitedRegionType.Filament);

        node.SW.Should().Be(VisitedRegionType.Filament);
        node.SE.Should().Be(VisitedRegionType.Unknown);
        node.NW.Should().Be(VisitedRegionType.Unknown);
        node.NE.Should().Be(VisitedRegionType.Unknown);

        node = node.WithSE(VisitedRegionType.Border);

        node.SW.Should().Be(VisitedRegionType.Filament);
        node.SE.Should().Be(VisitedRegionType.Border);
        node.NW.Should().Be(VisitedRegionType.Unknown);
        node.NE.Should().Be(VisitedRegionType.Unknown);

        node = node.WithNW(VisitedRegionType.Rejected);

        node.SW.Should().Be(VisitedRegionType.Filament);
        node.SE.Should().Be(VisitedRegionType.Border);
        node.NW.Should().Be(VisitedRegionType.Rejected);
        node.NE.Should().Be(VisitedRegionType.Unknown);

        node = node.WithNE(VisitedRegionType.Rejected);

        node.SW.Should().Be(VisitedRegionType.Filament);
        node.SE.Should().Be(VisitedRegionType.Border);
        node.NW.Should().Be(VisitedRegionType.Rejected);
        node.NE.Should().Be(VisitedRegionType.Rejected);
    }

    [Fact]
    public void ShouldUpdateChildrenWithQuadrant()
    {
        VisitNode.Unknown.WithQuadrant(Quadrant.SW, VisitedRegionType.Border).Should().Be(
            VisitNode.MakeLeaf(
                VisitedRegionType.Border,
                VisitedRegionType.Unknown,
                VisitedRegionType.Unknown,
                VisitedRegionType.Unknown));
        VisitNode.Unknown.WithQuadrant(Quadrant.SE, VisitedRegionType.Border).Should().Be(
            VisitNode.MakeLeaf(
                VisitedRegionType.Unknown,
                VisitedRegionType.Border,
                VisitedRegionType.Unknown,
                VisitedRegionType.Unknown));
        VisitNode.Unknown.WithQuadrant(Quadrant.NW, VisitedRegionType.Border).Should().Be(
            VisitNode.MakeLeaf(
                VisitedRegionType.Unknown,
                VisitedRegionType.Unknown,
                VisitedRegionType.Border,
                VisitedRegionType.Unknown));
        VisitNode.Unknown.WithQuadrant(Quadrant.NE, VisitedRegionType.Border).Should().Be(
            VisitNode.MakeLeaf(
                VisitedRegionType.Unknown,
                VisitedRegionType.Unknown,
                VisitedRegionType.Unknown,
                VisitedRegionType.Border));
    }

    [Theory]
    [InlineData(Quadrant.SW)]
    [InlineData(Quadrant.SE)]
    [InlineData(Quadrant.NW)]
    [InlineData(Quadrant.NE)]
    public void ShouldGetQuadrant(Quadrant quadrant)
    {
        VisitNode.Unknown
            .WithQuadrant(quadrant, VisitedRegionType.Border)
            .GetQuadrant(quadrant)
            .Should().Be(VisitedRegionType.Border);
    }
}