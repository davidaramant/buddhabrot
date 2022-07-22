using System.Drawing;
using Buddhabrot.Core;

namespace Buddhabrot.Extensions;

public static class DrawingExtensions
{
    public static bool IsInside(this Size size, Point p) =>
        p.X >= 0 && p.X < size.Width &&
        p.Y >= 0 && p.Y < size.Height;

    public static Point OffsetBy(this Point p, int xDelta, int yDelta) =>
        new(p.X + xDelta, p.Y + yDelta);

    public static int Area(this Size s) => s.Width * s.Height;

    public static Size Scale(this Size size, int scale) =>
        new(size.Width * scale, size.Height * scale);

    public static Point Scale(this Point point, int scale) =>
        new(point.X * scale, point.Y * scale);

    public static double DistanceSquaredFrom(this Point p1, Point p2) => (p1.X - p2.X) * (p1.X - p2.X) +
                                                                         (p1.Y - p2.Y) * (p1.Y - p2.Y);

    public static Point OffsetIn(this Point p, Direction direction) =>
        // Pixel locations use a different origin than Complex numbers.
        direction switch
        {
            Direction.Up => p.OffsetBy(0, -1),
            Direction.UpRight => p.OffsetBy(1, -1),
            Direction.Right => p.OffsetBy(1, 0),
            Direction.DownRight => p.OffsetBy(1, 1),
            Direction.Down => p.OffsetBy(0, 1),
            Direction.DownLeft => p.OffsetBy(-1, 1),
            Direction.Left => p.OffsetBy(-1, 0),
            Direction.UpLeft => p.OffsetBy(-1, -1),
            _ => throw new ArgumentException($"Unknown direction: {direction}")
        };

    public static Size HalveVertically(this Size size) => new(size.Width, size.Height / 2);

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