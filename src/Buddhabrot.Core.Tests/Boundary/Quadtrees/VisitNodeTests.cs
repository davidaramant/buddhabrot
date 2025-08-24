using Buddhabrot.Core.Boundary;
using Buddhabrot.Core.Boundary.Quadtrees;

namespace Buddhabrot.Core.Tests.Boundary.Quadtrees;

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

		node.NodeType.ShouldBe(VisitNodeType.Leaf);
		node.RegionType.ShouldBe(type);
	}

	[Fact]
	public void ShouldConstructLeafQuadCorrectly()
	{
		var node = VisitNode.MakeLeafQuad(
			VisitedRegionType.Filament,
			VisitedRegionType.Border,
			VisitedRegionType.Rejected,
			VisitedRegionType.Unknown
		);

		node.NodeType.ShouldBe(VisitNodeType.LeafQuad);

		node.SW.ShouldBe(VisitedRegionType.Filament);
		node.SE.ShouldBe(VisitedRegionType.Border);
		node.NW.ShouldBe(VisitedRegionType.Rejected);
		node.NE.ShouldBe(VisitedRegionType.Unknown);
	}

	[Fact]
	public void ShouldConstructBranchCorrectly()
	{
		var node = VisitNode.MakeBranch(123_456);

		node.NodeType.ShouldBe(VisitNodeType.Branch);

		node.ChildIndex.ShouldBe(123_456);
	}

	[Fact]
	public void ShouldChangeNodeTypeWhenSettingChildRegion()
	{
		var node = VisitNode.Unknown;

		node.NodeType.ShouldBe(VisitNodeType.Leaf);

		node = node.WithQuadrant(Quadrant.SW, VisitedRegionType.Border);

		node.NodeType.ShouldBe(VisitNodeType.LeafQuad);
	}

	[Fact]
	public void ShouldUpdateChildren()
	{
		var node = VisitNode.Unknown;

		node = node.WithSW(VisitedRegionType.Filament);

		node.SW.ShouldBe(VisitedRegionType.Filament);
		node.SE.ShouldBe(VisitedRegionType.Unknown);
		node.NW.ShouldBe(VisitedRegionType.Unknown);
		node.NE.ShouldBe(VisitedRegionType.Unknown);

		node = node.WithSE(VisitedRegionType.Border);

		node.SW.ShouldBe(VisitedRegionType.Filament);
		node.SE.ShouldBe(VisitedRegionType.Border);
		node.NW.ShouldBe(VisitedRegionType.Unknown);
		node.NE.ShouldBe(VisitedRegionType.Unknown);

		node = node.WithNW(VisitedRegionType.Rejected);

		node.SW.ShouldBe(VisitedRegionType.Filament);
		node.SE.ShouldBe(VisitedRegionType.Border);
		node.NW.ShouldBe(VisitedRegionType.Rejected);
		node.NE.ShouldBe(VisitedRegionType.Unknown);

		node = node.WithNE(VisitedRegionType.Rejected);

		node.SW.ShouldBe(VisitedRegionType.Filament);
		node.SE.ShouldBe(VisitedRegionType.Border);
		node.NW.ShouldBe(VisitedRegionType.Rejected);
		node.NE.ShouldBe(VisitedRegionType.Rejected);
	}

	[Fact]
	public void ShouldUpdateChildrenWithQuadrant()
	{
		VisitNode
			.Unknown.WithQuadrant(Quadrant.SW, VisitedRegionType.Border)
			.ShouldBe(
				VisitNode.MakeLeafQuad(
					VisitedRegionType.Border,
					VisitedRegionType.Unknown,
					VisitedRegionType.Unknown,
					VisitedRegionType.Unknown
				)
			);
		VisitNode
			.Unknown.WithQuadrant(Quadrant.SE, VisitedRegionType.Border)
			.ShouldBe(
				VisitNode.MakeLeafQuad(
					VisitedRegionType.Unknown,
					VisitedRegionType.Border,
					VisitedRegionType.Unknown,
					VisitedRegionType.Unknown
				)
			);
		VisitNode
			.Unknown.WithQuadrant(Quadrant.NW, VisitedRegionType.Border)
			.ShouldBe(
				VisitNode.MakeLeafQuad(
					VisitedRegionType.Unknown,
					VisitedRegionType.Unknown,
					VisitedRegionType.Border,
					VisitedRegionType.Unknown
				)
			);
		VisitNode
			.Unknown.WithQuadrant(Quadrant.NE, VisitedRegionType.Border)
			.ShouldBe(
				VisitNode.MakeLeafQuad(
					VisitedRegionType.Unknown,
					VisitedRegionType.Unknown,
					VisitedRegionType.Unknown,
					VisitedRegionType.Border
				)
			);
	}

	[Theory]
	[InlineData(Quadrant.SW)]
	[InlineData(Quadrant.SE)]
	[InlineData(Quadrant.NW)]
	[InlineData(Quadrant.NE)]
	public void ShouldGetQuadrant(Quadrant quadrant)
	{
		VisitNode
			.Unknown.WithQuadrant(quadrant, VisitedRegionType.Border)
			.GetQuadrant(quadrant)
			.ShouldBe(VisitedRegionType.Border);
	}
}
