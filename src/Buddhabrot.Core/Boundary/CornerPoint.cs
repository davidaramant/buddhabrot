namespace Buddhabrot.Core.Boundary;

public readonly record struct CornerPoint(int EncodedPosition)
{
    public int X => ushort.MaxValue & EncodedPosition;
    public int Y => EncodedPosition >> 16;

    public CornerPoint(int x, int y) : this((y << 16) + x)
    {
    }
}