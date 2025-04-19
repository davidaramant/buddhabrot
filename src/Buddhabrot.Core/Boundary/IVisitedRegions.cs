namespace Buddhabrot.Core.Boundary;

public interface IVisitedRegions
{
	bool Visit(RegionId id, VisitedRegionType type);
	bool HasVisited(RegionId id);
}
