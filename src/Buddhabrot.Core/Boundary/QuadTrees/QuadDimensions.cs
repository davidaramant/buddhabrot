namespace Buddhabrot.Core.Boundary.QuadTrees;

/// <summary>
/// Dimensions for a quad in region coordinates (+Y is up)
/// </summary>
public readonly record struct QuadDimensions(int X, int Y, int Height)
{
    public int SideLength => 1 << (Height - 1);
    public int QuadrantLength => 1 << (Height - 2);

    public bool Contains(RegionId id) => id.X >= X && id.Y >= Y && id.X < (X + SideLength) && id.Y < (Y + SideLength);

    /// <summary>
    /// Gets the region at the quadrant. Assumes that Height == 2
    /// </summary>
    public RegionId GetRegion(Quadrant quadrant)
    {
        var isLeft = (int)quadrant % 2;
        var isLower = (int)quadrant / 2;

        return new(X: X + isLeft * QuadrantLength, Y: Y + isLower * QuadrantLength);
    }

    public QuadDimensions Expand() => this with { Height = Height + 1 };

    public QuadDimensions SW => this with { Height = Height - 1 };
    public QuadDimensions SE => this with { X = X + QuadrantLength, Height = Height - 1 };
    public QuadDimensions NW => this with { Y = Y + QuadrantLength, Height = Height - 1 };
    public QuadDimensions NE => new(X: X + QuadrantLength, Y: Y + QuadrantLength, Height: Height - 1);

    public Quadrant DetermineQuadrant(int x, int y)
    {
        var xComponent = x / QuadrantLength;
        var yComponent = (y / QuadrantLength) << 1;

        return (Quadrant)(xComponent + yComponent);
    }

    public override string ToString() => $"{{X: {X}, Y:{Y}, Height: {Height}}}";
}
