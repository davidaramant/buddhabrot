using SkiaSharp;

namespace Buddhabrot.Core.Boundary;

/// <summary>
/// A viewport into a quad tree. In screen coordinates (+Y is down)
/// </summary>
/// <remarks>
/// The top left corner is an offset from the logical top left corner of the quad tree, which is (0,0). These are
/// screen coordinates but don't necessarily have the the same origin.
/// </remarks>
public readonly record struct QuadTreeViewport(SKPointI TopLeft, int Scale)
{
	public bool IsPoint => Scale == 0;
	public int Length => 1 << Scale;
	public int QuadrantLength => 1 << (Scale - 1);
	public SKPointI Center => new(TopLeft.X + QuadrantLength, TopLeft.Y + QuadrantLength);

	public QuadTreeViewport OffsetBy(int deltaX, int deltaY) =>
		new(new SKPointI(TopLeft.X + deltaX, TopLeft.Y + deltaY), Scale);

	public QuadTreeViewport ZoomIn(int x, int y) =>
		new(Scale: Scale + 1, TopLeft: new SKPointI(x: 2 * TopLeft.X - x, y: 2 * TopLeft.Y - y));

	public QuadTreeViewport ZoomOut(int width, int height) =>
		new(
			Scale: Scale - 1,
			TopLeft: new SKPointI(
				x: TopLeft.X - (TopLeft.X - (width / 2)) / 2,
				y: TopLeft.Y - (TopLeft.Y - (height / 2)) / 2
			)
		);

	public QuadTreeViewport SW => new(new SKPointI(TopLeft.X, TopLeft.Y + QuadrantLength), Scale - 1);
	public QuadTreeViewport SE => new(new SKPointI(TopLeft.X + QuadrantLength, TopLeft.Y + QuadrantLength), Scale - 1);
	public QuadTreeViewport NW => this with { Scale = Scale - 1 };
	public QuadTreeViewport NE => new(new SKPointI(TopLeft.X + QuadrantLength, TopLeft.Y), Scale - 1);

	public SKRectI IntersectWith(SKRectI rect)
	{
		int length = Length;

		int x1 = Math.Max(rect.Left, TopLeft.X);
		int x2 = Math.Min(rect.Right, TopLeft.X + length);
		int y1 = Math.Max(rect.Top, TopLeft.Y);
		int y2 = Math.Min(rect.Bottom, TopLeft.Y + length);

		return new SKRectI(x1, y1, x2, y2);
	}

	public override string ToString() => $"({TopLeft}), SideLength = {1 << Scale}";

	public static QuadTreeViewport GetLargestCenteredSquareInside(int width, int height)
	{
		var scale = Utility.GetLargestPowerOfTwoLessThan(Math.Min(width, height));

		var length = 1 << scale;

		var x = (width - length) / 2;
		var y = (height - length) / 2;
		return new(new SKPointI(x, y), scale);
	}
}
