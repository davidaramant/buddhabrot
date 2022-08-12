namespace Buddhabrot.Core.Boundary.Corners;

public sealed record CornersInSet(
    bool LowerLeft,
    bool LowerRight,
    bool UpperRight,
    bool UpperLeft)
{
    public bool IsUpperEdge => UpperLeft != UpperRight;
    public bool IsLowerEdge => LowerLeft != LowerRight;
    public bool IsLeftEdge => LowerLeft != UpperLeft;
    public bool IsRightEdge => LowerRight != UpperRight;

    public bool ContainsBorder => IsUpperEdge || IsLowerEdge || IsLeftEdge || IsRightEdge;
}