using System.Collections;

namespace Buddhabrot.Core.Boundary;

public interface IRegionsToCheck : IEnumerable<RegionId>
{
	void Add(RegionId region);
}

public sealed class RegionsToCheck : IRegionsToCheck
{
	private readonly Queue<RegionId> _regionsToCheck = new();

	public void Add(RegionId region) => _regionsToCheck.Enqueue(region);

	public IEnumerator<RegionId> GetEnumerator()
	{
		while (_regionsToCheck.Count > 0)
		{
			yield return _regionsToCheck.Dequeue();
		}
	}

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
