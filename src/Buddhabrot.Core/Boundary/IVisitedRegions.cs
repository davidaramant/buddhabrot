namespace Buddhabrot.Core.Boundary;

public interface IVisitedRegions
{
    int Count { get; }

    void Add(RegionId id, RegionType type);
    bool Contains(RegionId id);
}