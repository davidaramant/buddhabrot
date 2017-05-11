using System;
using System.Drawing;
using Buddhabrot.Core;

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

        public static Point OffsetBy(this Point p, int xDelta, int yDelta) =>
            new Point(p.X + xDelta, p.Y + yDelta);

        public static int Area(this Size s) => s.Width * s.Height;

        public static Size MultiplyBy(this Size size, int scale) =>
            new Size(size.Width * scale, size.Height * scale);

        public static Point OffsetIn(this Point p, Direction direction)
        {
            switch (direction)
            {
                case Direction.Up:
                    return p.OffsetBy(0, 1);
                case Direction.UpRight:
                    return p.OffsetBy(1, 1);
                case Direction.Right:
                    return p.OffsetBy(1, 0);
                case Direction.DownRight:
                    return p.OffsetBy(1, -1);
                case Direction.Down:
                    return p.OffsetBy(0, -1);
                case Direction.DownLeft:
                    return p.OffsetBy(-1, -1);
                case Direction.Left:
                    return p.OffsetBy(-1, 0);
                default:
                    throw new ArgumentException();
            }
        }

        public static Size HalveVertically(this Size size) => new Size(size.Width, size.Height / 2);
    }
}
