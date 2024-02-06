using Buddhabrot.Core.Boundary;
using Buddhabrot.Core.Boundary.QuadTrees;

namespace Buddhabrot.Core.Tests.Boundary.QuadTrees;

public class QuadDimensionTests
{
	[Theory]
	[InlineData(1, 1)]
	[InlineData(2, 2)]
	[InlineData(3, 4)]
	[InlineData(4, 8)]
	public void ShouldReturnCorrectSideLengthFromHeight(int height, int expectedSideLength)
	{
		var qd = new QuadDimensions(0, 0, height);
		qd.SideLength.Should().Be(expectedSideLength);
	}

	[Theory]
	[InlineData(0, 0, true)]
	[InlineData(1, 0, true)]
	[InlineData(0, 1, true)]
	[InlineData(1, 1, true)]
	[InlineData(2, 0, false)]
	[InlineData(0, 2, false)]
	public void ShouldDetermineIfRegionIsContainedByDimensions(int x, int y, bool shouldBeContained)
	{
		var qd = new QuadDimensions(0, 0, Height: 2);
		qd.Contains(new RegionId(x, y)).Should().Be(shouldBeContained);
	}

	[Theory]
	[InlineData(0, 0, Quadrant.SW)]
	[InlineData(1, 0, Quadrant.SE)]
	[InlineData(0, 1, Quadrant.NW)]
	[InlineData(1, 1, Quadrant.NE)]
	public void ShouldDetermineQuadrant(int x, int y, Quadrant expected)
	{
		var d = new QuadDimensions(0, 0, 2);
		d.DetermineQuadrant(x, y).Should().Be(expected);
	}

	[Fact]
	public void ShouldDescendHeight()
	{
		var height = 4;
		var x = 5;
		var y = 3;
		var quadLength = 1 << (height - 2);

		var quadrants = new List<Quadrant>();

		while (height > 1)
		{
			var h = x / quadLength;
			var v = y / quadLength;

			quadrants.Add((Quadrant)(h + (v << 1)));
			x -= h * quadLength;
			y -= v * quadLength;
			quadLength /= 2;
			height--;
		}

		quadrants.Should().BeEquivalentTo(new[] { Quadrant.SE, Quadrant.NW, Quadrant.NE });
	}
}
