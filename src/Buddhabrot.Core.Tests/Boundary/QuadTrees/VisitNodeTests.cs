using Buddhabrot.Core.Boundary;
using Buddhabrot.Core.Boundary.QuadTrees;

namespace Buddhabrot.Core.Tests.Boundary.QuadTrees;

public class VisitNodeTests
{
    [Theory]
    [InlineData(RegionType.Empty)]
    [InlineData(RegionType.Border)]
    [InlineData(RegionType.Filament)]
    [InlineData(RegionType.Rejected)]
    [InlineData(RegionType.InSet)]
    public void ShouldConstructLeafCorrectly(RegionType type)
    {
        var node = VisitNode.MakeLeaf(type);

        node.NodeType.Should().Be(VisitNodeType.Leaf);
        node.RegionType.Should().Be(type);
    }

    [Fact]
    public void ShouldConstructLeafQuadCorrectly()
    {
        var node = VisitNode.MakeLeaf(
            RegionType.Filament,
            RegionType.Border,
            RegionType.Rejected,
            RegionType.Empty);

        node.NodeType.Should().Be(VisitNodeType.LeafQuad);

        node.SW.Should().Be(RegionType.Filament);
        node.SE.Should().Be(RegionType.Border);
        node.NW.Should().Be(RegionType.Rejected);
        node.NE.Should().Be(RegionType.Empty);
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

        node = node.WithQuadrant(Quadrant.SW, RegionType.Border);

        node.NodeType.Should().Be(VisitNodeType.LeafQuad);
    }

    [Fact]
    public void ShouldUpdateChildren()
    {
        var node = VisitNode.Empty;

        node = node.WithSW(RegionType.Filament);

        node.SW.Should().Be(RegionType.Filament);
        node.SE.Should().Be(RegionType.Empty);
        node.NW.Should().Be(RegionType.Empty);
        node.NE.Should().Be(RegionType.Empty);

        node = node.WithSE(RegionType.Border);

        node.SW.Should().Be(RegionType.Filament);
        node.SE.Should().Be(RegionType.Border);
        node.NW.Should().Be(RegionType.Empty);
        node.NE.Should().Be(RegionType.Empty);

        node = node.WithNW(RegionType.Rejected);

        node.SW.Should().Be(RegionType.Filament);
        node.SE.Should().Be(RegionType.Border);
        node.NW.Should().Be(RegionType.Rejected);
        node.NE.Should().Be(RegionType.Empty);

        node = node.WithNE(RegionType.Rejected);

        node.SW.Should().Be(RegionType.Filament);
        node.SE.Should().Be(RegionType.Border);
        node.NW.Should().Be(RegionType.Rejected);
        node.NE.Should().Be(RegionType.Rejected);
    }

    [Fact]
    public void ShouldUpdateChildrenWithQuadrant()
    {
        VisitNode.Empty.WithQuadrant(Quadrant.SW, RegionType.Border).Should().Be(
            VisitNode.MakeLeaf(
                RegionType.Border,
                RegionType.Empty,
                RegionType.Empty,
                RegionType.Empty));
        VisitNode.Empty.WithQuadrant(Quadrant.SE, RegionType.Border).Should().Be(
            VisitNode.MakeLeaf(
                RegionType.Empty,
                RegionType.Border,
                RegionType.Empty,
                RegionType.Empty));
        VisitNode.Empty.WithQuadrant(Quadrant.NW, RegionType.Border).Should().Be(
            VisitNode.MakeLeaf(
                RegionType.Empty,
                RegionType.Empty,
                RegionType.Border,
                RegionType.Empty));
        VisitNode.Empty.WithQuadrant(Quadrant.NE, RegionType.Border).Should().Be(
            VisitNode.MakeLeaf(
                RegionType.Empty,
                RegionType.Empty,
                RegionType.Empty,
                RegionType.Border));
    }

    [Theory]
    [InlineData(Quadrant.SW)]
    [InlineData(Quadrant.SE)]
    [InlineData(Quadrant.NW)]
    [InlineData(Quadrant.NE)]
    public void ShouldGetQuadrant(Quadrant quadrant)
    {
        VisitNode.Empty
            .WithQuadrant(quadrant, RegionType.Border)
            .GetQuadrant(quadrant)
            .Should().Be(RegionType.Border);
    }
}