using System.Drawing;
using SkiaSharp;

namespace Buddhabrot.Core.Boundary;

// TODO: This data structure is amazingly confusing - how do you get one of these from a Complex number?

/// <summary>
/// A square in screen coordinates (+Y is down)
/// </summary>
public readonly record struct SquareBoundary(int X, int Y, int Scale)
{
	public bool IsPoint => Scale == 0;
	public int Length => 1 << Scale;
	public int QuadrantLength => 1 << (Scale - 1);
	public Point Center => new(X + QuadrantLength, Y + QuadrantLength);

	public SquareBoundary OffsetBy(int deltaX, int deltaY) => new(X + deltaX, Y + deltaY, Scale);

	public SquareBoundary ZoomIn(int x, int y) => new(Scale: Scale + 1, X: X - (x - X), Y: Y - (y - Y));

	public SquareBoundary ZoomOut(int width, int height) =>
		new(Scale: Scale - 1, X: X - (X - (width / 2)) / 2, Y: Y - (Y - (height / 2)) / 2);

	public SquareBoundary SW => new(X, Y + QuadrantLength, Scale - 1);
	public SquareBoundary SE => new(X + QuadrantLength, Y + QuadrantLength, Scale - 1);
	public SquareBoundary NW => this with { Scale = Scale - 1 };
	public SquareBoundary NE => new(X + QuadrantLength, Y, Scale - 1);

	public Rectangle IntersectWith(Rectangle rect)
	{
		int length = Length;

		int x1 = Math.Max(rect.X, X);
		int x2 = Math.Min(rect.X + rect.Width, X + length);
		int y1 = Math.Max(rect.Y, Y);
		int y2 = Math.Min(rect.Y + rect.Height, Y + length);

		return new Rectangle(x1, y1, x2 - x1, y2 - y1);
	}

	public Rectangle IntersectWith(SKRectI rect)
	{
		int length = Length;

		int x1 = Math.Max(rect.Left, X);
		int x2 = Math.Min(rect.Right, X + length);
		int y1 = Math.Max(rect.Top, Y);
		int y2 = Math.Min(rect.Bottom, Y + length);

		return new Rectangle(x1, y1, x2 - x1, y2 - y1);
	}

	public override string ToString() => $"({X}, {Y}), SideLength = {1 << Scale}";

	public static SquareBoundary GetLargestCenteredSquareInside(int width, int height)
	{
		var scale = Utility.GetLargestPowerOfTwoLessThan(Math.Min(width, height));

		var length = 1 << scale;

		var x = (width - length) / 2;
		var y = (height - length) / 2;
		return new(x, y, scale);
	}
}
