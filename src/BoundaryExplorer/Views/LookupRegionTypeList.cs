using System.Collections.Generic;
using System.Linq;
using Buddhabrot.Core.Boundary;

namespace BoundaryExplorer.Views;

/// <summary>
/// A run-length encoded list of region types.
/// </summary>
/// <remarks>
/// Very specialized. After it starts being consumed, it will not have more things added to it.
/// </remarks>
public sealed class LookupRegionTypeList
{
	private readonly List<LookupRegionType> _types = new();
	private readonly List<int> _counts = new();

	private int _index;

	public void Add(LookupRegionType type, int length)
	{
		if (!_types.Any() || _types[^1] != type)
		{
			_types.Add(type);
			_counts.Add(length);
		}
		else
		{
			_counts[^1] += length;
		}
	}

	public LookupRegionType GetNextType()
	{
		if (_counts[_index] == 0)
		{
			_index++;
		}

		_counts[_index]--;
		return _types[_index];
	}
}
