namespace Buddhabrot.Core.Boundary;

/// <summary>
/// Dimensions for a quad in region coordinates (+Y is up)
/// </summary>
public readonly record struct QuadDimensions(
    int X,
    int Y,
    int Height)
{
    public bool IsPoint => Height == 1;
    public int SideLength => 1 << (Height - 1);
    public int QuadrantLength => 1 << (Height - 2);

    public bool Contains(RegionId id) =>
        id.X >= X &&
        id.Y >= Y &&
        id.X < (X + SideLength) &&
        id.Y < (Y + SideLength);

    public QuadDimensions Expand() => this with {Height = Height + 1};
    public QuadDimensions LL => this with {Height = Height - 1};
    public QuadDimensions LR => this with {X = X + QuadrantLength, Height = Height - 1};
    public QuadDimensions UL => this with {Y = Y + QuadrantLength, Height = Height - 1};
    public QuadDimensions UR => new(X: X + QuadrantLength, Y: Y + QuadrantLength, Height: Height - 1);

    public Quadrant DetermineQuadrant(RegionId id)
    {
        var upper = id.Y >= Y + QuadrantLength;
        var right = id.X >= X + QuadrantLength;

        return (upper, right) switch
        {
            (false, false) => Quadrant.LL,
            (false, true) => Quadrant.LR,
            (true, false) => Quadrant.UL,
            (true, true) => Quadrant.UR,
        };
    }

    public QuadDimensions GetQuadrant(Quadrant quadrant) =>
        quadrant switch
        {
            Quadrant.LL => LL,
            Quadrant.LR => LR,
            Quadrant.UL => UL,
            Quadrant.UR => UR,
            _ => throw new Exception("Can't happen")
        };

    public override string ToString() => $"{{X: {X}, Y:{Y}, Height: {Height}}}";
}