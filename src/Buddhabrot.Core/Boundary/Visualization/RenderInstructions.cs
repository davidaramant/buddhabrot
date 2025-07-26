using SkiaSharp;

namespace Buddhabrot.Core.Boundary.Visualization;

public sealed class RenderInstructions : IEquatable<RenderInstructions>
{
	public enum Type
	{
		Nothing,
		Everything,
		FromViewport,
		ZoomIn,
		ZoomOut,
		Resize,
		Move,
	}

	private readonly SKRectI? _firstDirtyRect;
	private readonly SKRectI? _secondDirtyRect;

	public Type Operation { get; }
	public bool PasteFrontBuffer { get; }
	public SKRectI SourceRect { get; }
	public SKRectI DestRect { get; }
	public SKSizeI Size => QuadtreeViewport.Area.Size;
	public QuadtreeViewport QuadtreeViewport { get; }
	public ComplexViewport ComplexViewport { get; }

	private RenderInstructions(
		Type operation,
		bool pasteFrontBuffer,
		SKRectI sourceRect,
		SKRectI destRect,
		SKRectI? firstDirtyRect,
		SKRectI? secondDirtyRect,
		QuadtreeViewport quadtreeViewport
	)
	{
		Operation = operation;
		PasteFrontBuffer = pasteFrontBuffer;
		SourceRect = sourceRect;
		DestRect = destRect;
		_firstDirtyRect = firstDirtyRect;
		_secondDirtyRect = secondDirtyRect;
		QuadtreeViewport = quadtreeViewport;

		ComplexViewport = ComplexViewport.FromResolution(
			QuadtreeViewport.Area.Size,
			QuadtreeViewport.Center,
			2d / QuadtreeViewport.QuadrantLength
		);
	}

	public static readonly RenderInstructions Nothing = new(
		Type.Nothing,
		false,
		SKRectI.Empty,
		SKRectI.Empty,
		null,
		null,
		QuadtreeViewport.Empty
	);

	public static RenderInstructions Everything(SKSizeI newSize) =>
		new(
			operation: Type.Everything,
			pasteFrontBuffer: false,
			sourceRect: SKRectI.Empty,
			destRect: SKRectI.Empty,
			firstDirtyRect: SKRectI.Create(0, 0, newSize.Width, newSize.Height),
			secondDirtyRect: null,
			quadtreeViewport: QuadtreeViewport.GetLargestCenteredSquareInside(newSize)
		);

	public static RenderInstructions FromViewport(QuadtreeViewport viewport) =>
		new(
			operation: Type.FromViewport,
			pasteFrontBuffer: false,
			sourceRect: SKRectI.Empty,
			destRect: SKRectI.Empty,
			firstDirtyRect: SKRectI.Create(0, 0, viewport.Area.Width, viewport.Area.Height),
			secondDirtyRect: null,
			quadtreeViewport: viewport
		);

	public RenderInstructions ZoomIn(int x, int y) =>
		new(
			operation: Type.ZoomIn,
			pasteFrontBuffer: false,
			sourceRect: SKRectI.Empty,
			destRect: SKRectI.Empty,
			firstDirtyRect: SKRectI.Create(SKPointI.Empty, QuadtreeViewport.Area.Size),
			secondDirtyRect: null,
			quadtreeViewport: QuadtreeViewport.ZoomIn(x, y)
		);

	public RenderInstructions ZoomOut() =>
		new(
			operation: Type.ZoomOut,
			pasteFrontBuffer: false,
			sourceRect: SKRectI.Empty,
			destRect: SKRectI.Empty,
			firstDirtyRect: SKRectI.Create(SKPointI.Empty, QuadtreeViewport.Area.Size),
			secondDirtyRect: null,
			quadtreeViewport: QuadtreeViewport.ZoomOut()
		);

	public RenderInstructions Resize(SKSizeI newSize)
	{
		SKRectI? horizontal = null;
		if (newSize.Width > Size.Width)
		{
			horizontal = SKRectI.Create(x: Size.Width, y: 0, width: newSize.Width - Size.Width, height: newSize.Height);
		}

		SKRectI? vertical = null;
		if (newSize.Height > Size.Height)
		{
			vertical = SKRectI.Create(x: 0, y: Size.Height, width: Size.Width, height: newSize.Height - Size.Height);
		}

		var pasteRect = SKRectI.Create(
			0,
			0,
			width: Math.Min(Size.Width, newSize.Width),
			height: Math.Min(Size.Height, newSize.Height)
		);

		return new RenderInstructions(
			operation: Type.Resize,
			pasteFrontBuffer: true,
			sourceRect: pasteRect,
			destRect: pasteRect,
			firstDirtyRect: horizontal,
			secondDirtyRect: vertical,
			quadtreeViewport: QuadtreeViewport with
			{
				Area = SKRectI.Create(QuadtreeViewport.Area.Location, newSize),
			}
		);
	}

	public RenderInstructions Move(PositionOffset offset)
	{
		SKRectI? horizontal = null;
		if (offset.X != 0)
		{
			horizontal =
				offset.X < 0
					? SKRectI.Create(Size.Width + offset.X, 0, width: Math.Abs(offset.X), Size.Height)
					: SKRectI.Create(0, 0, width: offset.X, Size.Height);
		}

		SKRectI? vertical = null;
		if (offset.Y != 0)
		{
			if (offset.Y < 0) // Up
			{
				vertical = SKRectI.Create(
					x: Math.Max(0, offset.X),
					y: Size.Height + offset.Y,
					width: Math.Min(Size.Width, Size.Width - Math.Abs(offset.X)),
					height: Math.Abs(offset.Y)
				);
			}
			else // Down
			{
				vertical = SKRectI.Create(
					x: Math.Max(0, offset.X),
					y: 0,
					width: Math.Min(Size.Width, Size.Width - Math.Abs(offset.X)),
					height: offset.Y
				);
			}
		}

		var pasteWidth = Size.Width - Math.Abs(offset.X);
		var pasteHeight = Size.Height - Math.Abs(offset.Y);

		return new RenderInstructions(
			operation: Type.Move,
			pasteFrontBuffer: true,
			sourceRect: SKRectI.Create(
				x: ClampSource(offset.X),
				y: ClampSource(offset.Y),
				width: pasteWidth,
				height: pasteHeight
			),
			destRect: SKRectI.Create(
				x: ClampDest(offset.X),
				y: ClampDest(offset.Y),
				width: pasteWidth,
				height: pasteHeight
			),
			firstDirtyRect: horizontal,
			secondDirtyRect: vertical,
			quadtreeViewport: QuadtreeViewport.OffsetBy(offset)
		);

		static int ClampSource(int p) => -Math.Min(0, p);
		static int ClampDest(int p) => Math.Max(0, p);
	}

	public IEnumerable<SKRectI> GetDirtyRectangles()
	{
		if (_firstDirtyRect.HasValue)
			yield return _firstDirtyRect.Value;

		if (_secondDirtyRect.HasValue)
			yield return _secondDirtyRect.Value;
	}

	#region Equality (generated)

	public bool Equals(RenderInstructions? other)
	{
		if (ReferenceEquals(null, other))
			return false;
		if (ReferenceEquals(this, other))
			return true;
		return Nullable.Equals(_firstDirtyRect, other._firstDirtyRect)
			&& Nullable.Equals(_secondDirtyRect, other._secondDirtyRect)
			&& PasteFrontBuffer == other.PasteFrontBuffer
			&& SourceRect.Equals(other.SourceRect)
			&& DestRect.Equals(other.DestRect)
			&& Size.Equals(other.Size);
	}

	public override bool Equals(object? obj) =>
		ReferenceEquals(this, obj) || obj is RenderInstructions other && Equals(other);

	public override int GetHashCode() =>
		HashCode.Combine(_firstDirtyRect, _secondDirtyRect, PasteFrontBuffer, SourceRect, DestRect, Size);

	public static bool operator ==(RenderInstructions? left, RenderInstructions? right) => Equals(left, right);

	public static bool operator !=(RenderInstructions? left, RenderInstructions? right) => !Equals(left, right);

	#endregion
}
