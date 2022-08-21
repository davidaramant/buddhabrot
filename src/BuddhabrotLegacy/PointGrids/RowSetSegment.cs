namespace Buddhabrot.PointGrids;

/// <summary>
/// A segment of points that are in the set.
/// </summary>
public readonly struct RowSetSegment
{
    public readonly int StartCol;
    public readonly int Length;

    public RowSetSegment(int startCol, int length)
    {
        StartCol = startCol;
        Length = length;
    }
}