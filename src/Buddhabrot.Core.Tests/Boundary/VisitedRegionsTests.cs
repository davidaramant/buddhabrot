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

		tree.HasVisited(new RegionId(x, y)).Should().BeFalse();
	}

	[Fact]
	public void ShouldMarkRegionAsVisited()
	{
		var tree = new VisitedRegions();

		tree.Visit(new RegionId(0, 0), VisitedRegionType.Border);

		tree.HasVisited(new RegionId(0, 0)).Should().BeTrue();
		tree.HasVisited(new RegionId(1, 0)).Should().BeFalse();
		tree.HasVisited(new RegionId(0, 1)).Should().BeFalse();
		tree.HasVisited(new RegionId(1, 1)).Should().BeFalse();
	}

	[Fact]
	public void ShouldExpandTree()
	{
		var tree = new VisitedRegions();

		tree.Height.Should().Be(3);

		tree.Visit(new RegionId(4, 0), VisitedRegionType.Border);

		tree.Height.Should().Be(4);

		tree.HasVisited(new RegionId(4, 0)).Should().BeTrue();
	}

	[Fact]
	public void ShouldReturnBoundaryRegions()
	{
		var tree = new VisitedRegions();
		foreach (var i in Enumerable.Range(0, 8))
		{
			tree.Visit(new RegionId(i, i), (VisitedRegionType)(i % 4));
		}

		var boundary = tree.GetBoundaryRegions();
		boundary.Should().BeEquivalentTo(new[] { new RegionId(1, 1), new RegionId(5, 5) });
	}
}
