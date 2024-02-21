namespace Buddhabrot.Core.Boundary;

public interface IVisitedRegions
{
    void Visit(RegionId id, VisitedRegionType type);
    bool HasVisited(RegionId id);
}
