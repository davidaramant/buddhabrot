namespace Buddhabrot.Core.Tests;

public class ComplexAreaTests
{
    public static IEnumerable<object[]> GetOverlapData()
    {
        // Not overlapping
        yield return new object[]
        {
            new ComplexArea(new Interval(0, 1), new Interval(0, 1)),
            new ComplexArea(new Interval(2, 3), new Interval(2, 3)),
            false
        };
        // Equal
        yield return new object[]
        {
            new ComplexArea(new Interval(0, 1), new Interval(0, 1)),
            new ComplexArea(new Interval(0, 1), new Interval(0, 1)),
            true
        };
        // Partially overlapping
        yield return new object[]
        {
            new ComplexArea(new Interval(0, 2), new Interval(0, 2)),
            new ComplexArea(new Interval(1, 3), new Interval(1, 3)),
            true
        };
        // Area 2 inside of area 1
        yield return new object[]
        {
            new ComplexArea(new Interval(0, 10), new Interval(0, 10)),
            new ComplexArea(new Interval(1, 3), new Interval(1, 3)),
            true
        };
    }

    [Theory]
    [MemberData(nameof(GetOverlapData))]
    public void ShouldDetermineIfAreasOverlap(
        ComplexArea area1,
        ComplexArea area2,
        bool shouldIntersect)
    {
        area1.OverlapsWith(area2).Should().Be(shouldIntersect);
        area2.OverlapsWith(area1).Should().Be(shouldIntersect);
    }

    [Fact]
    public void ShouldDetermineQuadrants()
    {
        var area = new ComplexArea(new Interval(-1, 1), new Interval(-1, 1));

        area.GetNWQuadrant().Should().Be(new ComplexArea(new Interval(-1, 0), new Interval(0, 1)));
        area.GetNEQuadrant().Should().Be(new ComplexArea(new Interval(0, 1), new Interval(0, 1)));
        area.GetSEQuadrant().Should().Be(new ComplexArea(new Interval(0, 1), new Interval(-1, 0)));
        area.GetSWQuadrant().Should().Be(new ComplexArea(new Interval(-1, 0), new Interval(-1, 0)));
    }

    public sealed record IntersectionData(
        ComplexArea Area1,
        ComplexArea Area2,
        ComplexArea Expected,
        string Description)
    {
        public override string ToString() => Description;
    }

    public static IEnumerable<object[]> GetIntersectionData()
    {
        yield return new object[]
        {
            new IntersectionData(
                Area1: new ComplexArea(new Interval(0, 1), new Interval(0, 1)),
                Area2: new ComplexArea(new Interval(0, 1), new Interval(0, 1)),
                Expected: new ComplexArea(new Interval(0, 1), new Interval(0, 1)),
                Description: "Same area")
        };
        yield return new object[]
        {
            new IntersectionData(
                Area1: new ComplexArea(new Interval(0, 1), new Interval(0, 1)),
                Area2: new ComplexArea(new Interval(2, 3), new Interval(2, 3)),
                Expected: ComplexArea.Empty, 
                Description: "Distinct")
        };
        yield return new object[]
        {
            new IntersectionData(
                Area1: new ComplexArea(new Interval(0, 10), new Interval(0, 10)),
                Area2: new ComplexArea(new Interval(2, 3), new Interval(2, 3)),
                Expected: new ComplexArea(new Interval(2, 3), new Interval(2, 3)), 
                Description: "Subset")
        };
        yield return new object[]
        {
            new IntersectionData(
                Area1: new ComplexArea(new Interval(0, 2), new Interval(0, 2)),
                Area2: new ComplexArea(new Interval(1, 3), new Interval(1, 3)),
                Expected: new ComplexArea(new Interval(1, 2), new Interval(1, 2)),
                Description: "Overlap")
        };
    }
    
    [Theory]
    [MemberData(nameof(GetIntersectionData))]
    public void ShouldCalculateIntersection(IntersectionData data)
    {
        data.Area1.Intersect(data.Area2).Should().Be(data.Expected);
        data.Area2.Intersect(data.Area1).Should().Be(data.Expected);
    }
}