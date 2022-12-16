using System.Drawing;
using Buddhabrot.Core.Boundary;

namespace Buddhabrot.Core.Tests.Boundary;

public class SquareBoundaryTests
{
    [Fact]
    public void ShouldGetExpectedQuadrants()
    {
        var sq = new SquareBoundary(0, 0, 1);

        sq.NW.Should().Be(new SquareBoundary(0, 0, 0));
        sq.NE.Should().Be(new SquareBoundary(1, 0, 0));
        sq.SE.Should().Be(new SquareBoundary(1, 1, 0));
        sq.SW.Should().Be(new SquareBoundary(0, 1, 0));
    }

    public static IEnumerable<object[]> IntersectionData()
    {
        yield return new object[]
        {
            new Rectangle(new Point(10, 10), new Size(10, 10)),
            Rectangle.Empty
        };
        yield return new object[]
        {
            new Rectangle(new Point(0, 0), new Size(10, 10)),
            new Rectangle(new Point(0, 0), new Size(2, 2))
        };
    }

    [Theory]
    [MemberData(nameof(IntersectionData))]
    public void ShouldIntersectWithRectangle(Rectangle rect, Rectangle expected)
    {
        var sq = new SquareBoundary(-2, -2, 2);
        sq.IntersectWith(rect).Should().Be(expected);
    }

    [Fact]
    public void ShouldCalculateCenteredSquareInArea()
    {
        SquareBoundary.GetLargestCenteredSquareInside(10, 12).Should().Be(new SquareBoundary(1, 2, 3));
    }

    public sealed record ZoomOutTestCase(
        string Name,
        SquareBoundary Boundary,
        int Width,
        int Height,
        SquareBoundary ExpectedResult)
    {
        public override string ToString() => Name;
    }

    public static IEnumerable<object[]> ZoomOutData()
    {
        yield return new object[]
        {
            new ZoomOutTestCase("Centered",
                new SquareBoundary(X: 1, Y: 1, Scale: 3),
                Width: 10,
                Height: 10,
                ExpectedResult: new SquareBoundary(X: 3, Y: 3, Scale: 2))
        };
        yield return new object[]
        {
            new ZoomOutTestCase("Offset - Top Left",
                new SquareBoundary(X: 1, Y: 1, Scale: 3),
                Width: 20,
                Height: 20,
                ExpectedResult: new SquareBoundary(X: 5, Y: 5, Scale: 2))
        };
        yield return new object[]
        {
            new ZoomOutTestCase("Offset - Top Right",
                new SquareBoundary(X: 11, Y: 1, Scale: 3),
                Width: 20,
                Height: 20,
                ExpectedResult: new SquareBoundary(X: 11, Y: 5, Scale: 2))
        };
    }

    [Theory]
    [MemberData(nameof(ZoomOutData))]
    public void ShouldZoomOutCorrectly(ZoomOutTestCase testCase) =>
        testCase.Boundary.ZoomOut(width: testCase.Width, height: testCase.Height).Should().Be(testCase.ExpectedResult);
}