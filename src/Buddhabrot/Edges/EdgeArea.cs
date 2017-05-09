using System.Collections.Generic;
using System.Drawing;

namespace Buddhabrot.Edges
{
    public sealed class EdgeArea
    {
        public Point GridLocation { get; }
        public Corners CornersInSet { get; }

        public EdgeArea(Point location, Corners cornersInSet)
        {
            GridLocation = location;
            CornersInSet = cornersInSet;
        }

        public IEnumerable<Corners> GetCornersInSet()
        {
            if (CornersInSet.HasFlag(Corners.TopRight))
                yield return Corners.TopRight;
            if (CornersInSet.HasFlag(Corners.TopLeft))
                yield return Corners.TopLeft;
            if (CornersInSet.HasFlag(Corners.BottomRight))
                yield return Corners.BottomRight;
            if (CornersInSet.HasFlag(Corners.BottomLeft))
                yield return Corners.BottomLeft;
        }

        public IEnumerable<Corners> GetCornersNotInSet()
        {
            if (!CornersInSet.HasFlag(Corners.TopRight))
                yield return Corners.TopRight;
            if (!CornersInSet.HasFlag(Corners.TopLeft))
                yield return Corners.TopLeft;
            if (!CornersInSet.HasFlag(Corners.BottomRight))
                yield return Corners.BottomRight;
            if (!CornersInSet.HasFlag(Corners.BottomLeft))
                yield return Corners.BottomLeft;
        }

        public override string ToString() => $"Location: {GridLocation}, Corners in Set: {CornersInSet}";
    }
}
