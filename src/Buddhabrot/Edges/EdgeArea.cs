using System.Drawing;

namespace Buddhabrot.Edges
{
    public sealed class EdgeArea
    {
        public Point GridLocation { get; }
        public Size Dimensions { get; }

        public EdgeArea(Point location) : this(location, new Size(1, 1))
        {           
        }

        public EdgeArea(Point location, Size dimensions)
        {
            GridLocation = location;
            Dimensions = dimensions;
        }
    }
}
