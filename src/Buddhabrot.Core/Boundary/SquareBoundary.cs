using System.Drawing;

namespace Buddhabrot.Core.Boundary;

public readonly record struct SquareBoundary(
    Point TopLeft,
    int Scale)
{
    public int X => TopLeft.X;
    public int Y => TopLeft.Y;
    public bool IsPoint => Scale == 0;
    public int QuadrantLength => 1 << (Scale - 1);

    public SquareBoundary GetNWQuadrant() => new(TopLeft, Scale - 1);
    public SquareBoundary GetNEQuadrant() => new(TopLeft + new Size(QuadrantLength, 0), Scale - 1);
    public SquareBoundary GetSEQuadrant() => new(TopLeft + new Size(QuadrantLength, QuadrantLength), Scale - 1);
    public SquareBoundary GetSWQuadrant() => new(TopLeft + new Size(0, QuadrantLength), Scale - 1);

    public Rectangle IntersectWith(Rectangle rect)
    {
        int length = 1 << Scale;

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

    public override string ToString() => $"{TopLeft}, SideLength = {1 << Scale}";
}