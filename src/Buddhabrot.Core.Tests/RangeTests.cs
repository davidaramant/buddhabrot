namespace Buddhabrot.Core.Tests;

public class RangeTests
{
    [Theory]
    [InlineData(0, 1, 2, 3, false)] // Distinct
    [InlineData(0, 1, 0, 1, true)] // Equal
    [InlineData(0, 10, 2, 3, true)] // One contained in other
    [InlineData(0, 2, 1, 3, true)] // Partial overlap
    public void ShouldDetermineIfRangesOverlap(int r1Min, int r1Max, int r2Min, int r2Max, bool shouldIntersect)
    {
        var range1 = new Range(r1Min, r1Max);
        var range2 = new Range(r2Min, r2Max);
        range1.OverlapsWith(range2).Should().Be(shouldIntersect);
        range2.OverlapsWith(range1).Should().Be(shouldIntersect);
    }

    [Theory]
    [InlineData(0, 2, 1)]
    [InlineData(-2, -1, -1.5)]
    public void ShouldGetFirstAndLastHalf(double min, double max, double midPoint)
    {
        var range = new Range(min, max);
        range.FirstHalf().Should().Be(new Range(min, midPoint));
        range.LastHalf().Should().Be(new Range(midPoint, max));
    }
}