namespace Buddhabrot.Core.Boundary;

// TODO: This should be benchmarked.
// It is faster than a single massive HashSet for large amounts of data, but what is the transition?
// What if the interiors HashSets were replaced with something like Interval? Would the cost of maintaining it be slower?
// Would a quad tree be faster?
public sealed class VisitedRegions : IVisitedRegions
{
    // Rows of columns
    private readonly List<HashSet<int>> _rows;

    public VisitedRegions(int numRows)
    {
        _rows = new(numRows);
    }

    public int Count => _rows.Sum(col => col.Count);

    public void Add(RegionId id, RegionType _)
    {
        // This method is safe because the boundary scanning starts at row 0 and can't skip rows
        // We cannot get into situations where it will reference a row that doesn't exist

        if (id.Y >= _rows.Count)
        {
            _rows.Add(new HashSet<int>());
        }

        _rows[id.Y].Add(id.X);
    }

    public bool Contains(RegionId id)
    {
        if (id.Y >= _rows.Count)
            return false;

        return _rows[id.Y].Contains(id.X);
    }
}
