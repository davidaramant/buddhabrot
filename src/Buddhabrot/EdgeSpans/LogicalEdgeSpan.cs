using Buddhabrot.Core;

namespace Buddhabrot.EdgeSpans
{
    /// <summary>
    /// The location of a point in the set and the direction to a point outside.
    /// </summary>
    public struct LogicalEdgeSpan
    {
        public readonly int X;
        public readonly int Y;
        public readonly Direction ToOutside;

        public LogicalEdgeSpan(int x, int y, Direction toOutside)
        {
            X = x;
            Y = y;
            ToOutside = toOutside;
        }
    }
}
