using System.Drawing;
using System.Numerics;

namespace Buddhabrot.Core.Tests;

public sealed class ViewPortTests
{
    [Fact]
    public void ShouldFigureOutMiddleOfSquareArea()
    {
        var viewPort = new ViewPort(
            new ComplexArea(
                new Interval(-1, 1),
                new Interval(-1, 1)),
            new Size(101, 101));

        var middle = viewPort.GetPosition(new Complex());

        middle.Should().Be(new Point(50, 50));
    }

    [Fact]
    public void ShouldUseTopLeftAsPositionOrigin()
    {
        var viewPort = new ViewPort(
            new ComplexArea(
                new Interval(-1, 1),
                new Interval(-1, 1)),
            new Size(101, 101));

        var topLeft = viewPort.GetPosition(new Complex(-1, 1));

        topLeft.Should().Be(new Point(0, 0));
    }

    [Fact]
    public void ShouldRoundTripPositions()
    {
        var viewPort = new ViewPort(
            new ComplexArea(
                new Interval(-1, 1),
                new Interval(-1, 1)),
            new Size(101, 101));

        var c = new Complex(-1, 1);

        var roundTripped = viewPort.GetComplex(viewPort.GetPosition(c));
        roundTripped.Should().Be(c);
    }

    [Fact]
    public void ShouldRoundTripComplexNumbers()
    {
        var viewPort = new ViewPort(
            new ComplexArea(
                new Interval(-1, 1),
                new Interval(-1, 1)),
            new Size(101, 101));

        var point = new Point(-1, 1);

        var roundTripped = viewPort.GetPosition(viewPort.GetComplex(point));
        roundTripped.Should().Be(point);
    }
}