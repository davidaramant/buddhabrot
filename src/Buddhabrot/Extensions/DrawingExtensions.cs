using System;
using System.Collections.Generic;
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

        public static Size Scale(this Size size, int scale) =>
            new Size(size.Width * scale, size.Height * scale);

        public static Point Scale(this Point point, int scale) =>
            new Point(point.X * scale, point.Y * scale);

        public static double DistanceSquaredFrom(this Point p1, Point p2) => (p1.X - p2.X) * (p1.X - p2.X) +
                                                                             (p1.Y - p2.Y) * (p1.Y - p2.Y);

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
                case Direction.UpLeft:
                    return p.OffsetBy(-1, 1);
                default:
                    throw new ArgumentException($"Unknown direction: {direction}");
            }
        }

        public static Size HalveVertically(this Size size) => new Size(size.Width, size.Height / 2);

        public static IEnumerable<Point> GetPositionsRowFirst(this Size resolution)
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
