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
}