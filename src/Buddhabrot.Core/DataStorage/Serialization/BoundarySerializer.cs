using Buddhabrot.Core.Boundary;
using Buddhabrot.Core.DataStorage.Serialization.Internal;

namespace Buddhabrot.Core.DataStorage.Serialization;

public static class BoundarySerializer
{
    public static void Save(
        BoundaryParameters parameters,
        IEnumerable<RegionId> regions,
        Stream stream)
    {
        var boundaries = new Boundaries
        {
            VerticalPower = parameters.Divisions.VerticalPower,
            MaximumIterations = parameters.MaxIterations,
            Regions = regions.Select(r => new RegionLocation { X = r.X, Y = r.Y }).ToArray(),
        };
        boundaries.Save(stream);
    }

    public static void Save(
        RegionLookup lookup,
        Stream stream)
    {
        var quadTree = new PersistedQuadTree
        {
            Height = lookup.Levels,
            Nodes = lookup.GetRawNodes().Select(n => n.Encoded).ToArray(),
        };
        quadTree.Save(stream);
    }

    public static (BoundaryParameters Parameters, IReadOnlyList<RegionId> Regions) LoadRegions(Stream stream)
    {
        var boundaries = Boundaries.Load(stream);
        return (
            new BoundaryParameters(new AreaDivisions(boundaries.VerticalPower), boundaries.MaximumIterations),
            boundaries.Regions.Select(rl => new RegionId(X: rl.X, Y: rl.Y)).ToList());
    }

    public static RegionLookup LoadQuadTree(Stream stream)
    {
        var quadTree = PersistedQuadTree.Load(stream);
        return new RegionLookup(
            quadTree.Height,
            quadTree.Nodes);
    }
}