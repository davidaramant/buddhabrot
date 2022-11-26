namespace Buddhabrot.Core.Boundary;

public interface IVisitedRegions
{
    void MarkVisited(RegionId id, RegionType type);
    bool HasVisited(RegionId id);
}