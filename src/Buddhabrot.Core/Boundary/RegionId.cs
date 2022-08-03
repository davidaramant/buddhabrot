namespace Buddhabrot.Core.Boundary;

/// <summary>
/// Id of a region..
/// </summary>
/// <remarks>
/// Origin is the top left (-2 + 0i). Increasing Y goes DOWN
/// </remarks>
public readonly record struct RegionId(int EncodedPosition)
{
    public int X => ushort.MaxValue & EncodedPosition;
    public int Y => EncodedPosition >> 16;

    public RegionId(int x, int y) : this((y << 16) + x)
    {
    }

    public override string ToString() => $"Region ({X}, {Y})";

    public RegionId MoveUp() => new(X, Y - 1);
    public RegionId MoveDown() => new(X, Y + 1);
    public RegionId MoveLeft() => new(X - 1, Y);
    public RegionId MoveRight() => new(X + 1, Y);

    public CornerId UpperLeftCorner() => new(X, Y);
    public CornerId UpperRightCorner() => new(X + 1, Y);
    public CornerId LowerRightCorner() => new(X + 1, Y + 1);
    public CornerId LowerLeftCorner() => new(X, Y + 1);
}