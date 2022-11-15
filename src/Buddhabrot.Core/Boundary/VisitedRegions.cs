namespace Buddhabrot.Core.Boundary;

// TODO: This should be benchmarked.
// It is faster than a single massive HashSet for large amounts of data, but what is the transition?
// What if the interiors HashSets were replaced with something like Interval? Would the cost of maintaining it be slower?
// Would a quad tree be faster?
public sealed class VisitedRegions
{
    private readonly List<HashSet<int>> _cache = new();

    public int Count => _cache.Sum(col => col.Count);

    public void Add(RegionId id)
    {
        if (id.Y >= _cache.Count)
        {
            _cache.Add(new HashSet<int>());
        }

        _cache[id.Y].Add(id.X);
    }

    public bool Contains(RegionId id)
    {
        if (id.Y >= _cache.Count)
            return false;

        return _cache[id.Y].Contains(id.X);
    }
}
