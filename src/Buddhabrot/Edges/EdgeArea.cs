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

    }
}
