using Buddhabrot.Core.ExtensionMethods.Drawing;
using SkiaSharp;

namespace Buddhabrot.Core.Boundary;

/// <summary>
/// A viewport into a quad tree. In screen coordinates (+Y is down)
/// </summary>
/// <remarks>
/// The top left corner is an offset from the logical top left corner of the quad tree, which is (0,0). These are
/// screen coordinates but don't necessarily have the same origin.
/// </remarks>
public readonly record struct QuadtreeViewport(SKRectI Area, int Scale)
{
	public static readonly QuadtreeViewport Empty = new(SKRectI.Empty, 0);

	public bool IsPoint => Scale == 0;
	public int Length => 1 << Scale;
	public int QuadrantLength => 1 << (Scale - 1);
	public SKPointI Center => new(Area.Left + QuadrantLength, Area.Top + QuadrantLength);

	public QuadtreeViewport OffsetBy(PositionOffset offset) => this with { Area = Area.OffsetBy(offset.X, offset.Y) };

	public QuadtreeViewport OffsetBy(int deltaX, int deltaY) => this with { Area = Area.OffsetBy(deltaX, deltaY) };

	public QuadtreeViewport ZoomIn(int x, int y) =>
		new(Scale: Scale + 1, Area: SKRectI.Create(new SKPointI(x: 2 * Area.Left - x, y: 2 * Area.Top - y), Area.Size));

	public QuadtreeViewport ZoomOut() =>
		new(
			Scale: Scale - 1,
			Area: SKRectI.Create(
				new SKPointI(
					x: Area.Left - (Area.Left - (Area.Size.Width / 2)) / 2,
					y: Area.Top - (Area.Top - (Area.Size.Height / 2)) / 2
				),
				Area.Size
			)
		);

	public QuadtreeViewport SW =>
		new(SKRectI.Create(new SKPointI(Area.Left, Area.Top + QuadrantLength), Area.Size.Quarter()), Scale - 1);
	public QuadtreeViewport SE =>
		new(
			SKRectI.Create(new SKPointI(Area.Left + QuadrantLength, Area.Top + QuadrantLength), Area.Size.Quarter()),
			Scale - 1
		);
	public QuadtreeViewport NW => new(SKRectI.Create(Area.Location, Area.Size.Quarter()), Scale - 1);
	public QuadtreeViewport NE =>
		new(SKRectI.Create(new SKPointI(Area.Left + QuadrantLength, Area.Top), Area.Size.Quarter()), Scale - 1);

	public SKRectI IntersectWith(SKRectI rect)
	{
		int length = Length;

		int x1 = Math.Max(rect.Left, Area.Left);
		int x2 = Math.Min(rect.Right, Area.Left + length);
		int y1 = Math.Max(rect.Top, Area.Top);
		int y2 = Math.Min(rect.Bottom, Area.Top + length);

		return new SKRectI(x1, y1, x2, y2);
	}

	public override string ToString() => $"({Area}), SideLength = {1 << Scale}";

	public static QuadtreeViewport GetLargestCenteredSquareInside(SKSizeI size)
	{
		var scale = Utility.GetLargestPowerOfTwoLessThan(Math.Min(size.Width, size.Height));

		var length = 1 << scale;

		var x = (size.Width - length) / 2;
		var y = (size.Height - length) / 2;
		return new(SKRectI.Create(new SKPointI(x, y), size), scale);
	}
}
