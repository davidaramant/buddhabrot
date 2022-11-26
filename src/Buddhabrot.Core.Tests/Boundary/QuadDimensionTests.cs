using Buddhabrot.Core.Boundary;

namespace Buddhabrot.Core.Tests.Boundary;

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
    [InlineData(0, 0, Quadrant.LL)]
    [InlineData(1, 0, Quadrant.LR)]
    [InlineData(0, 1, Quadrant.UL)]
    [InlineData(1, 1, Quadrant.UR)]
    public void ShouldDetermineQuadrant(int x, int y, Quadrant expected)
    {
        var d = new QuadDimensions(0, 0, 2);
        d.DetermineQuadrant(new RegionId(x, y)).Should().Be(expected);
    }

    [Theory]
    [InlineData(Quadrant.LL, 0, 0)]
    [InlineData(Quadrant.LR, 1, 0)]
    [InlineData(Quadrant.UL, 0, 1)]
    [InlineData(Quadrant.UR, 1, 1)]
    public void ShouldGetQuadrant(Quadrant quadrant, int expectedX, int expectedY)
    {
        var d = new QuadDimensions(0, 0, 2).GetQuadrant(quadrant);
        d.Height.Should().Be(1);
        d.X.Should().Be(expectedX);
        d.Y.Should().Be(expectedY);
    }
}