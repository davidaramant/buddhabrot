namespace Buddhabrot.Core.Boundary;

/// <summary>
/// Id of a region.
/// </summary>
/// <remarks>
/// Origin is -2 + 0i. Increasing Y goes UP
/// </remarks>
public readonly record struct RegionId(int X, int Y)
{
    public static RegionId FromEncodedPosition(int encodedPosition) =>
        new(encodedPosition & ushort.MaxValue, encodedPosition >> 16);

    public override string ToString() => $"Region ({X}, {Y})";

    public RegionId MoveUp() => new(X, Y + 1);
    public RegionId MoveDown() => new(X, Y - 1);
    public RegionId MoveLeft() => new(X - 1, Y);
    public RegionId MoveRight() => new(X + 1, Y);

    public RegionId MoveUpLeft() => new(X - 1, Y + 1);
    public RegionId MoveUpRight() => new(X + 1, Y + 1);
    public RegionId MoveDownLeft() => new(X - 1, Y - 1);
    public RegionId MoveDownRight() => new(X + 1, Y - 1);
}