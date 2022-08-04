using Buddhabrot.Core.Boundary;
using Buddhabrot.Core.DataStorage.Serialization.Internal;

namespace Buddhabrot.Core.DataStorage.Serialization;

public static class BoundarySerializer
{
    public static void Save(BoundaryParameters parameters, IEnumerable<RegionId> regions, Stream stream)
    {
        var boundaries = new Boundaries
        {
            VerticalDivisions = parameters.VerticalDivisions,
            MaximumIterations = parameters.MaxIterations,
            Regions = regions.Select(r => new RegionLocation {X = r.X, Y = r.Y}).ToArray(),
        };
        boundaries.Save(stream);
    }

    public static (BoundaryParameters Parameters, IReadOnlyList<RegionId> Regions) Load(Stream stream)
    {
        var boundaries = Boundaries.Load(stream);
        return (
            new BoundaryParameters(boundaries.VerticalDivisions, boundaries.MaximumIterations),
            boundaries.EncodedRegions.Select(RegionId.FromEncodedPosition)
                .Concat(boundaries.Regions.Select(rl => new RegionId(X: rl.X, Y: rl.Y))).ToList());
    }
}