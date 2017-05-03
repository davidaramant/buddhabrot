using System.Collections.Generic;
using System.Drawing;

namespace Buddhabrot.Extensions
{
    public static class DrawingExtensions
    {
        public static bool IsInside(this Size size, Point p)
        {
            return
                p.X >= 0 && p.X < size.Width &&
                p.Y >= 0 && p.Y < size.Height;
        }

        public static Point OffsetBy(this Point p, int xDelta, int yDelta)
        {
            return new Point(p.X + xDelta, p.Y + yDelta);
        }
    }
}
