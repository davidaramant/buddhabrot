using System.Drawing;

namespace Buddhabrot.Core.Boundary;

public readonly record struct SquareBoundary(
    int X,
    int Y,
    int Scale)
{
    public bool IsPoint => Scale == 0;
    public int Length => 1 << Scale;
    public int QuadrantLength => 1 << (Scale - 1);
    public Point Center => new(X + QuadrantLength, Y + QuadrantLength);

    public SquareBoundary OffsetBy(int deltaX, int deltaY) => new(X + deltaX, Y + deltaY, Scale);

    // TODO: Take into account position when zooming in
    public SquareBoundary ZoomIn(int x, int y) => this with {Scale = Scale + 1};
    // TODO: Take into account position when zooming out
    public SquareBoundary ZoomOut() => this with {Scale = Scale - 1};

    public SquareBoundary GetNWQuadrant() => this with {Scale = Scale - 1};
    public SquareBoundary GetNEQuadrant() => new(X + QuadrantLength, Y, Scale - 1);
    public SquareBoundary GetSEQuadrant() => new(X + QuadrantLength, Y + QuadrantLength, Scale - 1);
    public SquareBoundary GetSWQuadrant() => new(X, Y + QuadrantLength, Scale - 1);

    public Rectangle IntersectWith(Rectangle rect)
    {
        int length = Length;

        int x1 = Math.Max(rect.X, X);
        int x2 = Math.Min(rect.X + rect.Width, X + length);
        int y1 = Math.Max(rect.Y, Y);
        int y2 = Math.Min(rect.Y + rect.Height, Y + length);

        if (x2 >= x1 && y2 >= y1)
        {
            return new Rectangle(x1, y1, x2 - x1, y2 - y1);
        }

        return Rectangle.Empty;
    }

    public override string ToString() => $"({X}, {Y}), SideLength = {1 << Scale}";

    public static SquareBoundary GetLargestCenteredSquareInside(int width, int height)
    {
        var scale = Utility.GetLargestPowerOfTwoLessThan(Math.Min(width, height));

        var length = 1 << scale;

        var x = (width - length) / 2;
        var y = (height - length) / 2;
        return new(x, y, scale);
    }
}