namespace Buddhabrot.Core.Boundary;

public readonly record struct CornerId(int EncodedPosition)
{
    public int X => ushort.MaxValue & EncodedPosition;
    public int Y => EncodedPosition >> 16;

    public CornerId(int x, int y) : this((y << 16) + x)
    {
    }
}