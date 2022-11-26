namespace Buddhabrot.Core.Boundary;

public interface IVisitedRegions
{
    int Count { get; }

    void MarkVisited(RegionId id, RegionType type);
    bool HasVisited(RegionId id);
}