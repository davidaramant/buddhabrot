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
            Regions = regions.Select(a => a.EncodedPosition).ToArray(),
        };
        boundaries.Save(stream);
    }

    public static (BoundaryParameters Parameters, IReadOnlyList<RegionId> Regions) Load(Stream stream)
    {
        var boundaries = Boundaries.Load(stream);
        return (
            new BoundaryParameters(boundaries.VerticalDivisions, boundaries.MaximumIterations),
            boundaries.Regions.Select(pos => new RegionId(pos)).ToList());
    }
}