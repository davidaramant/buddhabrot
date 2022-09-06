﻿using System.Drawing;
using Buddhabrot.Core.Boundary;

namespace Buddhabrot.Core.Tests.Boundary;

public class SquareBoundaryTests
{
    [Fact]
    public void ShouldGetExpectedQuadrants()
    {
        var sq = new SquareBoundary(0, 0, 1);

        sq.GetNWQuadrant().Should().Be(new SquareBoundary(0, 0, 0));
        sq.GetNEQuadrant().Should().Be(new SquareBoundary(1, 0, 0));
        sq.GetSEQuadrant().Should().Be(new SquareBoundary(1, 1, 0));
        sq.GetSWQuadrant().Should().Be(new SquareBoundary(0, 1, 0));
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
}