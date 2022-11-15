namespace Buddhabrot.Core.Boundary;

public interface IVisitedRegions
{
    int Count { get; }

    void Add(RegionId id);
    bool Contains(RegionId id);
}