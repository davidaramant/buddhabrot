namespace Buddhabrot.Core.Boundary.Corners;

public readonly record struct CornerId(int X, int Y)
{
    public static CornerId operator +(CornerId id, Offset offset) =>
        new(id.X + offset.X, id.Y + offset.Y);
}
