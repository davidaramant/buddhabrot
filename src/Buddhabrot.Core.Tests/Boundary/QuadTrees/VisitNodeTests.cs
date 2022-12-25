using Buddhabrot.Core.Boundary;
using Buddhabrot.Core.Boundary.QuadTrees;

namespace Buddhabrot.Core.Tests.Boundary.QuadTrees;

public class VisitNodeTests
{
    [Theory]
    [InlineData(VisitedRegionType.Empty)]
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
            VisitedRegionType.Empty);

        node.NodeType.Should().Be(VisitNodeType.LeafQuad);

        node.SW.Should().Be(VisitedRegionType.Filament);
        node.SE.Should().Be(VisitedRegionType.Border);
        node.NW.Should().Be(VisitedRegionType.Rejected);
        node.NE.Should().Be(VisitedRegionType.Empty);
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
        var node = VisitNode.Empty;

        node.NodeType.Should().Be(VisitNodeType.Leaf);

        node = node.WithQuadrant(Quadrant.SW, VisitedRegionType.Border);

        node.NodeType.Should().Be(VisitNodeType.LeafQuad);
    }

    [Fact]
    public void ShouldUpdateChildren()
    {
        var node = VisitNode.Empty;

        node = node.WithSW(VisitedRegionType.Filament);

        node.SW.Should().Be(VisitedRegionType.Filament);
        node.SE.Should().Be(VisitedRegionType.Empty);
        node.NW.Should().Be(VisitedRegionType.Empty);
        node.NE.Should().Be(VisitedRegionType.Empty);

        node = node.WithSE(VisitedRegionType.Border);

        node.SW.Should().Be(VisitedRegionType.Filament);
        node.SE.Should().Be(VisitedRegionType.Border);
        node.NW.Should().Be(VisitedRegionType.Empty);
        node.NE.Should().Be(VisitedRegionType.Empty);

        node = node.WithNW(VisitedRegionType.Rejected);

        node.SW.Should().Be(VisitedRegionType.Filament);
        node.SE.Should().Be(VisitedRegionType.Border);
        node.NW.Should().Be(VisitedRegionType.Rejected);
        node.NE.Should().Be(VisitedRegionType.Empty);

        node = node.WithNE(VisitedRegionType.Rejected);

        node.SW.Should().Be(VisitedRegionType.Filament);
        node.SE.Should().Be(VisitedRegionType.Border);
        node.NW.Should().Be(VisitedRegionType.Rejected);
        node.NE.Should().Be(VisitedRegionType.Rejected);
    }

    [Fact]
    public void ShouldUpdateChildrenWithQuadrant()
    {
        VisitNode.Empty.WithQuadrant(Quadrant.SW, VisitedRegionType.Border).Should().Be(
            VisitNode.MakeLeaf(
                VisitedRegionType.Border,
                VisitedRegionType.Empty,
                VisitedRegionType.Empty,
                VisitedRegionType.Empty));
        VisitNode.Empty.WithQuadrant(Quadrant.SE, VisitedRegionType.Border).Should().Be(
            VisitNode.MakeLeaf(
                VisitedRegionType.Empty,
                VisitedRegionType.Border,
                VisitedRegionType.Empty,
                VisitedRegionType.Empty));
        VisitNode.Empty.WithQuadrant(Quadrant.NW, VisitedRegionType.Border).Should().Be(
            VisitNode.MakeLeaf(
                VisitedRegionType.Empty,
                VisitedRegionType.Empty,
                VisitedRegionType.Border,
                VisitedRegionType.Empty));
        VisitNode.Empty.WithQuadrant(Quadrant.NE, VisitedRegionType.Border).Should().Be(
            VisitNode.MakeLeaf(
                VisitedRegionType.Empty,
                VisitedRegionType.Empty,
                VisitedRegionType.Empty,
                VisitedRegionType.Border));
    }

    [Theory]
    [InlineData(Quadrant.SW)]
    [InlineData(Quadrant.SE)]
    [InlineData(Quadrant.NW)]
    [InlineData(Quadrant.NE)]
    public void ShouldGetQuadrant(Quadrant quadrant)
    {
        VisitNode.Empty
            .WithQuadrant(quadrant, VisitedRegionType.Border)
            .GetQuadrant(quadrant)
            .Should().Be(VisitedRegionType.Border);
    }
}