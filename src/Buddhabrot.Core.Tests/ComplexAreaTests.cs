namespace Buddhabrot.Core.Tests;

public class ComplexAreaTests
{
    public static IEnumerable<object[]> GetIntersectionData()
    {
        // Not overlapping
        yield return new object[]
        {
            new ComplexArea(new Range(0, 1), new Range(0, 1)),
            new ComplexArea(new Range(2, 3), new Range(2, 3)),
            false
        };
        // Overlapping
        yield return new object[]
        {
            new ComplexArea(new Range(0, 2), new Range(0, 2)),
            new ComplexArea(new Range(1, 3), new Range(1, 3)),
            true
        };
        // Area 2 inside of area 1
        yield return new object[]
        {
            new ComplexArea(new Range(0, 10), new Range(0, 10)),
            new ComplexArea(new Range(1, 3), new Range(1, 3)),
            true
        };
    }

    [Theory]
    [MemberData(nameof(GetIntersectionData))]
    public void ShouldDetermineAreaIntersections(
        ComplexArea area1,
        ComplexArea area2,
        bool shouldIntersect)
    {
        area1.Intersects(area2).Should().Be(shouldIntersect);
        area2.Intersects(area1).Should().Be(shouldIntersect);
    }
}