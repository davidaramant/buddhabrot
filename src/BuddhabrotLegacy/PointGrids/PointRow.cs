namespace Buddhabrot.PointGrids;

/// <summary>
/// Stores whether the points in the row are inside the Mandelbrot set.
/// </summary>
public sealed class PointRow : IEnumerable<bool>
{
    // TODO: This should use RowSetSegment
    private readonly List<(bool inSet, int start, int length)> _segments = new();

    public int Y { get; }
    public int Width { get; }

    public bool this[int pointIndex]
    {
        get
        {
            var indexOfSegmentPastPointIndex = _segments.FindIndex(s => s.start > pointIndex);

            var segment =
                indexOfSegmentPastPointIndex == -1
                    ? _segments.Last()
                    : _segments[indexOfSegmentPastPointIndex - 1];

            return segment.inSet;
        }
    }

    public PointRow(int width, int y, IEnumerable<(bool inSet, int length)> segments)
    {
        Width = width;
        Y = y;

        int position = 0;
        foreach (var segment in segments)
        {
            _segments.Add((segment.inSet, position, segment.length));
            position += segment.length;
        }

        if (position != Width)
        {
            throw new ArgumentException($"The segments should have added up to {width} but reached {position}.");
        }
    }

    public IEnumerable<RowSetSegment> GetSegmentsInSet() => 
        _segments.Where(s => s.inSet).Select(s => new RowSetSegment(s.start, s.length));

    public IEnumerable<int> GetXPositionsOfSet() => 
        _segments.Where(s => s.inSet).SelectMany(segment => Enumerable.Range(segment.start, segment.length));

    public IEnumerator<bool> GetEnumerator() => 
        _segments.SelectMany(segment => Enumerable.Repeat(segment.inSet, segment.length)).GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}