namespace Buddhabrot.Core.Boundary;

/// <summary>
/// Id of a region..
/// </summary>
/// <remarks>
/// Origin is the top left (-2 + 0i). Increasing Y goes DOWN
/// </remarks>
public readonly record struct RegionId(int X, int Y)
{
    public static RegionId FromEncodedPosition(int encodedPosition) =>
        new(encodedPosition & ushort.MaxValue, encodedPosition >> 16);

    public override string ToString() => $"Region ({X}, {Y})";

    public RegionId Halve() => new(X / 2, Y / 2);
    public RegionId Double() => new(X * 2, Y * 2);
    
    public RegionId MoveUp() => new(X, Y - 1);
    public RegionId MoveDown() => new(X, Y + 1);
    public RegionId MoveLeft() => new(X - 1, Y);
    public RegionId MoveRight() => new(X + 1, Y);

    public CornerId UpperLeftCorner() => new(X, Y);
    public CornerId UpperRightCorner() => new(X + 1, Y);
    public CornerId LowerRightCorner() => new(X + 1, Y + 1);
    public CornerId LowerLeftCorner() => new(X, Y + 1);
}