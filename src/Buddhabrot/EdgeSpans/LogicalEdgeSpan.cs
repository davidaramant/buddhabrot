using System.Drawing;
using Buddhabrot.Core;
using Buddhabrot.Extensions;

namespace Buddhabrot.EdgeSpans;

/// <summary>
/// The location of a point in the set and the direction to a point outside.
/// </summary>
public readonly record struct LogicalEdgeSpan(
    Point Location,
    Direction ToOutside)
{
    public EdgeSpan ToConcreteDouble(ViewPort viewPort) => new(
        InSet: viewPort.GetComplex(Location),
        NotInSet: viewPort.GetComplex(Location.OffsetIn(ToOutside)));
    public FEdgeSpan ToConcreteFloat(ViewPort viewPort) => new(
        InSet: viewPort.GetComplex(Location).ToFloat(),
        NotInSet: viewPort.GetComplex(Location.OffsetIn(ToOutside)).ToFloat());
}