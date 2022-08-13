namespace Buddhabrot.Core.Boundary.RegionQuadTree;

public interface IRegionMap
{
    ComplexArea PopulatedArea { get; }
    IReadOnlyList<ComplexArea> GetVisibleAreas(ComplexArea searchArea);
}