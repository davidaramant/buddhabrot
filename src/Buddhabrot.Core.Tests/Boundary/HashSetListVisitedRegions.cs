using Buddhabrot.Core.Boundary;

namespace Buddhabrot.Core.Tests.Boundary;

public sealed class HashSetListVisitedRegions : IVisitedRegions
{
	// Rows of columns
	private readonly List<HashSet<int>> _rows;

	public HashSetListVisitedRegions(int numRows)
	{
		_rows = new(numRows);
	}

	public int Count => _rows.Sum(col => col.Count);

	public void Visit(RegionId id, VisitedRegionType _)
	{
		// This method is safe because the boundary scanning starts at row 0 and can't skip rows
		// We cannot get into situations where it will reference a row that doesn't exist

		if (id.Y >= _rows.Count)
		{
			_rows.Add(new HashSet<int>());
		}

		_rows[id.Y].Add(id.X);
	}

	public bool HasVisited(RegionId id) => id.Y < _rows.Count && _rows[id.Y].Contains(id.X);
}
