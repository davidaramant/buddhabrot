using System.Drawing;
using Buddhabrot.Core;

namespace Buddhabrot.Edges
{
    public sealed class EdgeArea
    {
        public ComplexArea Area { get; }
        public Point GridLocation { get; }
        public int Length { get; }
        public EncodingDirection EncodingDirection { get; }

        public EdgeArea(ComplexArea area, Point gridLocation) :
            this(area, gridLocation, EncodingDirection.None, 1)
        {
        }

        public EdgeArea(ComplexArea area, Point gridLocation, EncodingDirection encodingDirection, int length)
        {
            Area = area;
            GridLocation = gridLocation;
            EncodingDirection = encodingDirection;
            Length = length;
        }
    }
}
