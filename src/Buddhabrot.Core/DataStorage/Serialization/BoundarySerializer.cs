using Buddhabrot.Core.Boundary;
using Buddhabrot.Core.DataStorage.Serialization.Internal;

namespace Buddhabrot.Core.DataStorage.Serialization;

public static class BoundarySerializer
{
    public static void Save(
        BoundaryParameters parameters,
        IEnumerable<RegionId> regions,
        RegionLookup lookup,
        Stream stream)
    {
        var boundaries = new Boundaries
        {
            VerticalPower = parameters.Divisions.VerticalPower,
            MaximumIterations = parameters.MaxIterations,
            Regions = regions.Select(r => new RegionLocation { X = r.X, Y = r.Y }).ToArray(),
            MaxX = lookup.MaxX,
            MaxY = lookup.MaxY,
            QuadTreeNodes = lookup.GetRawNodes().Select(q => q.Encoded).ToArray(),
        };
        boundaries.Save(stream);
    }

    public static (BoundaryParameters Parameters, IReadOnlyList<RegionId> Regions, RegionLookup Lookup) Load(
        Stream stream)
    {
        var boundaries = Boundaries.Load(stream);
        return (
            new BoundaryParameters(new AreaDivisions(boundaries.VerticalPower), boundaries.MaximumIterations),
            boundaries.Regions.Select(rl => new RegionId(X: rl.X, Y: rl.Y)).ToList(),
            new RegionLookup(boundaries.VerticalPower, boundaries.MaxX, boundaries.MaxY, boundaries.QuadTreeNodes));
    }
}