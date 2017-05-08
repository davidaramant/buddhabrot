using System.Drawing;

namespace Buddhabrot.Edges
{
    public sealed class EdgeAreaLegacy
    {
        public Point GridLocation { get; }
        public Size Dimensions { get; }

        public EdgeAreaLegacy(Point location) : this(location, new Size(1, 1))
        {           
        }

        public EdgeAreaLegacy(Point location, Size dimensions)
        {
            GridLocation = location;
            Dimensions = dimensions;
        }
    }
}
