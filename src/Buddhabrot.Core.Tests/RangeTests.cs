namespace Buddhabrot.Core.Tests;

public class RangeTests
{
    [Theory]
    [InlineData(0, 1, 2, 3, false)]
    [InlineData(0, 10, 2, 3, true)]
    [InlineData(0, 2, 1, 3, true)]
    public void ShouldDetermineIntersections(int r1Min, int r1Max, int r2Min, int r2Max, bool shouldIntersect)
    {
        var range1 = new Range(r1Min, r1Max);
        var range2 = new Range(r2Min, r2Max);
        range1.Intersects(range2).Should().Be(shouldIntersect);
        range2.Intersects(range1).Should().Be(shouldIntersect);
    }
}