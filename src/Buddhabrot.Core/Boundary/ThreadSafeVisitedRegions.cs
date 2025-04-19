namespace Buddhabrot.Core.Boundary;

public sealed class ThreadSafeVisitedRegions : IVisitedRegions
{
	private readonly IVisitedRegions _visitedRegions;
	private readonly CancellationTokenSource _cancelSource;
	private readonly Lock _lock = new();

	public CancellationToken Token => _cancelSource.Token;

	public ThreadSafeVisitedRegions(IVisitedRegions visitedRegions, CancellationToken token)
	{
		_visitedRegions = visitedRegions;
		_cancelSource = CancellationTokenSource.CreateLinkedTokenSource(token);
	}

	public void Visit(RegionId id, VisitedRegionType type)
	{
		lock (_lock)
		{
			// TODO: Is it possible to make Visit return a bool to avoid traversing the tree twice?
			if (_visitedRegions.HasVisited(id))
			{
				_cancelSource.Cancel();
			}
			else
			{
				_visitedRegions.Visit(id, type);
			}
		}
	}

	public bool HasVisited(RegionId id)
	{
		lock (_lock)
		{
			return _visitedRegions.HasVisited(id);
		}
	}
}
