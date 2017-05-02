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

        public static IEnumerable<Point> GetAllPoints(this Size resolution)
        {
            for (int y = 0; y < resolution.Height; y++)
            {
                for (int x = 0; x < resolution.Width; x++)
                {
                    yield return new Point(x, y);
                }
            }
        }
    }
}
