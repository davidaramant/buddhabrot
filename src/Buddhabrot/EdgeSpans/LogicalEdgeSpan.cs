using System.Drawing;
using Buddhabrot.Core;
using Buddhabrot.Extensions;

namespace Buddhabrot.EdgeSpans
{
    /// <summary>
    /// The location of a point in the set and the direction to a point outside.
    /// </summary>
    public struct LogicalEdgeSpan
    {
        public readonly Point Location;
        public readonly Direction ToOutside;

        public LogicalEdgeSpan(Point location, Direction toOutside)
        {
            Location = location;
            ToOutside = toOutside;
        }

        public EdgeSpan ToConcreteDouble(ViewPort viewPort) => new EdgeSpan(
                inSet: viewPort.GetComplex(Location),
                notInSet: viewPort.GetComplex(Location.OffsetIn(ToOutside)));
        public FEdgeSpan ToConcreteFloat(ViewPort viewPort) => new FEdgeSpan(
            inSet: viewPort.GetComplex(Location).ToFloat(),
            notInSet: viewPort.GetComplex(Location.OffsetIn(ToOutside)).ToFloat());
    }
}
