using Buddhabrot.Core.Boundary;
using Buddhabrot.Core.Serialization.Internal;

namespace Buddhabrot.Core.Serialization;

public static class BoundarySerializer
{
    public static void Save(BoundaryParameters parameters, IEnumerable<AreaId> areas, Stream stream)
    {
        var boundaries = new Boundaries
        {
            VerticalDivisions = parameters.VerticalDivisions,
            MaximumIterations = parameters.MaxIterations,
            AreaIds = areas.Select(a => a.EncodedPosition).ToArray(),
        };
        boundaries.Save(stream);
    }

    public static (BoundaryParameters Parameters, IReadOnlyList<AreaId> AreaIds) Load(Stream stream)
    {
        var boundaries = Boundaries.Load(stream);
        return (
            new BoundaryParameters(boundaries.VerticalDivisions, boundaries.MaximumIterations),
            boundaries.AreaIds.Select(pos => new AreaId(pos)).ToList());
    }
}