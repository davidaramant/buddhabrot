using System.Drawing;
using Buddhabrot.Core;

namespace Buddhabrot.Edges
{
    public sealed class EdgeArea
    {
        public ComplexArea Area { get; }
        public Point GridLocation { get; }

        public EdgeArea(ComplexArea area, Point gridLocation)
        {
            Area = area;
            GridLocation = gridLocation;
        }
    }
}
