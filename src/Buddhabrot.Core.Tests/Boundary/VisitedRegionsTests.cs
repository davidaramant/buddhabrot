using Buddhabrot.Core.Boundary;

namespace Buddhabrot.Core.Tests.Boundary;

public class VisitedRegionsTests
{
	[Theory]
	[InlineData(0, 0)]
	[InlineData(0, 1)]
	[InlineData(1, 0)]
	[InlineData(1, 1)]
	[InlineData(2, 0)]
	[InlineData(0, 2)]
	public void HasVisitedShouldReturnFalseForEmptyTree(int x, int y)
	{
		var tree = new VisitedRegions();

		tree.HasVisited(new RegionId(x, y)).ShouldBeFalse();
	}

	[Fact]
	public void ShouldMarkRegionAsVisited()
	{
		var tree = new VisitedRegions();

		tree.Visit(new RegionId(0, 0), VisitedRegionType.Border);

		tree.HasVisited(new RegionId(0, 0)).ShouldBeTrue();
		tree.HasVisited(new RegionId(1, 0)).ShouldBeFalse();
		tree.HasVisited(new RegionId(0, 1)).ShouldBeFalse();
		tree.HasVisited(new RegionId(1, 1)).ShouldBeFalse();
	}

	[Theory]
	[InlineData(4, 4)]
	[InlineData(8, 5)]
	public void ShouldExpandTree(int x, int expectedHeight)
	{
		var tree = new VisitedRegions();

		tree.Height.ShouldBe(3);

		tree.Visit(new RegionId(x, 0), VisitedRegionType.Border);

		tree.Height.ShouldBe(expectedHeight);

		tree.HasVisited(new RegionId(x, 0)).ShouldBeTrue();
	}

	[Fact]
	public void ShouldDetermineIfRegionHasAlreadyBeenVisited()
	{
		var tree = new VisitedRegions();
		tree.Visit(new RegionId(8, 0), VisitedRegionType.Border).ShouldBeTrue();
		tree.Visit(new RegionId(8, 0), VisitedRegionType.Border).ShouldBeFalse();

		tree.Visit(new RegionId(9, 0), VisitedRegionType.Border).ShouldBeTrue();
		tree.Visit(new RegionId(9, 0), VisitedRegionType.Border).ShouldBeFalse();
	}

	[Fact]
	public void ShouldReturnBoundaryRegions()
	{
		var tree = new VisitedRegions();
		foreach (var i in Enumerable.Range(0, 8))
		{
			tree.Visit(new RegionId(i, i), (VisitedRegionType)(i % 4));
		}

		var boundary = tree.GetBorderRegions();
		boundary.ShouldBe([new RegionId(1, 1), new RegionId(5, 5)], ignoreOrder: true);
	}
}
