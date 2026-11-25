namespace Buddhabrot.Core.Tests;

public class IntervalTests
{
	[Test]
	[Arguments(0, 1, 2, 3, false)] // Distinct
	[Arguments(0, 1, 0, 1, true)] // Equal
	[Arguments(0, 10, 2, 3, true)] // One contained in other
	[Arguments(0, 2, 1, 3, true)] // Partial overlap
	public void ShouldDetermineIfRangesOverlap(int r1Min, int r1Max, int r2Min, int r2Max, bool shouldIntersect)
	{
		var range1 = new Interval(r1Min, r1Max);
		var range2 = new Interval(r2Min, r2Max);
		range1.OverlapsWith(range2).ShouldBe(shouldIntersect);
		range2.OverlapsWith(range1).ShouldBe(shouldIntersect);
	}

	[Test]
	[Arguments(0, 2, 1)]
	[Arguments(-2, -1, -1.5)]
	public void ShouldGetFirstAndLastHalf(double min, double max, double midPoint)
	{
		var range = new Interval(min, max);
		range.FirstHalf().ShouldBe(new Interval(min, midPoint));
		range.LastHalf().ShouldBe(new Interval(midPoint, max));
	}

	[Test]
	[Arguments(0, 1, 2, 3, 0, 0)] // Distinct
	[Arguments(0, 2, 1, 3, 1, 2)] // Partial overlap
	[Arguments(0, 10, 2, 3, 2, 3)] // Subset
	[Arguments(0, 1, 0, 1, 0, 1)] // Equal
	public void ShouldCalculateIntersection(
		double min1,
		double max1,
		double min2,
		double max2,
		double expectedMin,
		double expectedMax
	)
	{
		var range1 = new Interval(min1, max1);
		var range2 = new Interval(min2, max2);

		var expectedIntersection = new Interval(expectedMin, expectedMax);

		range1.Intersect(range2).ShouldBe(expectedIntersection);
		range2.Intersect(range1).ShouldBe(expectedIntersection);
	}

	[Test]
	[Arguments(0, 100, 0.5, 25, 75)]
	public void ShouldZoom(double min, double max, double zoom, double newMin, double newMax)
	{
		var range = new Interval(min, max);
		range.Scale(zoom).ShouldBe(new Interval(newMin, newMax));
	}
}
