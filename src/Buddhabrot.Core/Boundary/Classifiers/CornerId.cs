namespace Buddhabrot.Core.Boundary.Classifiers;

public readonly record struct CornerId(int X, int Y)
{
    public static CornerId operator +(CornerId id, Offset offset) => new(id.X + offset.X, id.Y + offset.Y);
}
