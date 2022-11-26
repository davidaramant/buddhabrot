namespace Buddhabrot.Core.Boundary;

public interface IVisitedRegions
{
    void Visit(RegionId id, RegionType type);
    bool HasVisited(RegionId id);
}