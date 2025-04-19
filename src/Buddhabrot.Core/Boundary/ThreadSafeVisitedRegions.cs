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

	public bool Visit(RegionId id, VisitedRegionType type)
	{
		lock (_lock)
		{
			if (!_visitedRegions.Visit(id, type))
			{
				_cancelSource.Cancel();
				return false;
			}
		}

		return true;
	}

	public bool HasVisited(RegionId id)
	{
		lock (_lock)
		{
			return _visitedRegions.HasVisited(id);
		}
	}
}
