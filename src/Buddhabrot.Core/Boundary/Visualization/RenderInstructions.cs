using System.Drawing;

namespace Buddhabrot.Core.Boundary.Visualization;

public sealed class RenderInstructions : IEquatable<RenderInstructions>
{
	private readonly Rectangle? _firstDirtyRect;
	private readonly Rectangle? _secondDirtyRect;

	public bool PasteFrontBuffer { get; }
	public Rectangle SourceRect { get; }
	public Rectangle DestRect { get; }
	public Size Size { get; }

	private RenderInstructions(
		bool pasteFrontBuffer,
		Rectangle sourceRect,
		Rectangle destRect,
		Rectangle? firstDirtyRect,
		Rectangle? secondDirtyRect,
		Size size
	)
	{
		PasteFrontBuffer = pasteFrontBuffer;
		SourceRect = sourceRect;
		DestRect = destRect;
		_firstDirtyRect = firstDirtyRect;
		_secondDirtyRect = secondDirtyRect;
		Size = size;
	}

	public static RenderInstructions Everything(Size newSize) =>
		new(
			pasteFrontBuffer: false,
			sourceRect: Rectangle.Empty,
			destRect: Rectangle.Empty,
			firstDirtyRect: new Rectangle(new Point(0, 0), newSize),
			secondDirtyRect: null,
			size: newSize
		);

	public static RenderInstructions Resized(Size oldSize, Size newSize)
	{
		Rectangle? horizontal = null;
		if (newSize.Width > oldSize.Width)
		{
			horizontal = new Rectangle(
				x: oldSize.Width,
				y: 0,
				width: newSize.Width - oldSize.Width,
				height: newSize.Height
			);
		}

		Rectangle? vertical = null;
		if (newSize.Height > oldSize.Height)
		{
			vertical = new Rectangle(
				x: 0,
				y: oldSize.Height,
				width: oldSize.Width,
				height: newSize.Height - oldSize.Height
			);
		}

		var pasteRect = new Rectangle(
			0,
			0,
			width: Math.Min(oldSize.Width, newSize.Width),
			height: Math.Min(oldSize.Height, newSize.Height)
		);

		return new RenderInstructions(
			pasteFrontBuffer: true,
			sourceRect: pasteRect,
			destRect: pasteRect,
			firstDirtyRect: horizontal,
			secondDirtyRect: vertical,
			size: newSize
		);
	}

	public static RenderInstructions Moved(Size size, PositionOffset offset)
	{
		Rectangle? horizontal = null;
		if (offset.X != 0)
		{
			if (offset.X < 0)
			{
				horizontal = new Rectangle(size.Width + offset.X, 0, width: Math.Abs(offset.X), size.Height);
			}
			else
			{
				horizontal = new Rectangle(0, 0, width: offset.X, size.Height);
			}
		}

		Rectangle? vertical = null;
		if (offset.Y != 0)
		{
			if (offset.Y < 0) // Up
			{
				vertical = new Rectangle(
					x: Math.Max(0, offset.X),
					y: size.Height + offset.Y,
					width: Math.Min(size.Width, size.Width - Math.Abs(offset.X)),
					height: Math.Abs(offset.Y)
				);
			}
			else // Down
			{
				vertical = new Rectangle(
					x: Math.Max(0, offset.X),
					y: 0,
					width: Math.Min(size.Width, size.Width - Math.Abs(offset.X)),
					height: offset.Y
				);
			}
		}

		var pasteWidth = size.Width - Math.Abs(offset.X);
		var pasteHeight = size.Height - Math.Abs(offset.Y);

		return new RenderInstructions(
			pasteFrontBuffer: true,
			sourceRect: new Rectangle(
				x: ClampSource(offset.X),
				y: ClampSource(offset.Y),
				width: pasteWidth,
				height: pasteHeight
			),
			destRect: new Rectangle(
				x: ClampDest(offset.X),
				y: ClampDest(offset.Y),
				width: pasteWidth,
				height: pasteHeight
			),
			firstDirtyRect: horizontal,
			secondDirtyRect: vertical,
			size: size
		);

		static int ClampSource(int p) => -Math.Min(0, p);
		static int ClampDest(int p) => Math.Max(0, p);
	}

	public IEnumerable<Rectangle> GetDirtyRectangles()
	{
		if (_firstDirtyRect.HasValue)
			yield return new Rectangle(
				_firstDirtyRect.Value.X,
				_firstDirtyRect.Value.Y,
				_firstDirtyRect.Value.Width,
				_firstDirtyRect.Value.Height
			);

		if (_secondDirtyRect.HasValue)
			yield return new Rectangle(
				_secondDirtyRect.Value.X,
				_secondDirtyRect.Value.Y,
				_secondDirtyRect.Value.Width,
				_secondDirtyRect.Value.Height
			);
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

	public override bool Equals(object? obj)
	{
		return ReferenceEquals(this, obj) || obj is RenderInstructions other && Equals(other);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(_firstDirtyRect, _secondDirtyRect, PasteFrontBuffer, SourceRect, DestRect, Size);
	}

	public static bool operator ==(RenderInstructions? left, RenderInstructions? right)
	{
		return Equals(left, right);
	}

	public static bool operator !=(RenderInstructions? left, RenderInstructions? right)
	{
		return !Equals(left, right);
	}

	#endregion
}
