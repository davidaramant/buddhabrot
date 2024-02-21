using System;
using System.Collections.Generic;
using Avalonia;

namespace BoundaryExplorer.Views;

public sealed class RenderInstructions : IEquatable<RenderInstructions>
{
	private readonly PixelRect? _firstDirtyRect;
	private readonly PixelRect? _secondDirtyRect;

	public bool PasteFrontBuffer { get; }
	public Rect SourceRect { get; }
	public Rect DestRect { get; }
	public PixelSize Size { get; }

	private RenderInstructions(
		bool pasteFrontBuffer,
		Rect sourceRect,
		Rect destRect,
		PixelRect? firstDirtyRect,
		PixelRect? secondDirtyRect,
		PixelSize size
	)
	{
		PasteFrontBuffer = pasteFrontBuffer;
		SourceRect = sourceRect;
		DestRect = destRect;
		_firstDirtyRect = firstDirtyRect;
		_secondDirtyRect = secondDirtyRect;
		Size = size;
	}

	public static RenderInstructions Everything(PixelSize newSize) =>
		new(
			pasteFrontBuffer: false,
			sourceRect: Rect.Empty,
			destRect: Rect.Empty,
			firstDirtyRect: new PixelRect(new PixelPoint(0, 0), newSize),
			secondDirtyRect: null,
			size: newSize
		);

	public static RenderInstructions Resized(PixelSize oldSize, PixelSize newSize)
	{
		PixelRect? horizontal = null;
		if (newSize.Width > oldSize.Width)
		{
			horizontal = new PixelRect(
				x: oldSize.Width,
				y: 0,
				width: newSize.Width - oldSize.Width,
				height: newSize.Height
			);
		}

		PixelRect? vertical = null;
		if (newSize.Height > oldSize.Height)
		{
			vertical = new PixelRect(
				x: 0,
				y: oldSize.Height,
				width: oldSize.Width,
				height: newSize.Height - oldSize.Height
			);
		}

		var pasteRect = new Rect(
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

	public static RenderInstructions Moved(PixelSize size, PixelVector offset)
	{
		PixelRect? horizontal = null;
		if (offset.X != 0)
		{
			if (offset.X < 0)
			{
				horizontal = new PixelRect(size.Width + offset.X, 0, width: Math.Abs(offset.X), size.Height);
			}
			else
			{
				horizontal = new PixelRect(0, 0, width: offset.X, size.Height);
			}
		}

		PixelRect? vertical = null;
		if (offset.Y != 0)
		{
			if (offset.Y < 0) // Up
			{
				vertical = new PixelRect(
					x: Math.Max(0, offset.X),
					y: size.Height + offset.Y,
					width: Math.Min(size.Width, size.Width - Math.Abs(offset.X)),
					height: Math.Abs(offset.Y)
				);
			}
			else // Down
			{
				vertical = new PixelRect(
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
			sourceRect: new Rect(
				x: ClampSource(offset.X),
				y: ClampSource(offset.Y),
				width: pasteWidth,
				height: pasteHeight
			),
			destRect: new Rect(x: ClampDest(offset.X), y: ClampDest(offset.Y), width: pasteWidth, height: pasteHeight),
			firstDirtyRect: horizontal,
			secondDirtyRect: vertical,
			size: size
		);

		static int ClampSource(int p) => -Math.Min(0, p);
		static int ClampDest(int p) => Math.Max(0, p);
	}

	public IEnumerable<System.Drawing.Rectangle> GetDirtyRectangles()
	{
		if (_firstDirtyRect.HasValue)
			yield return new System.Drawing.Rectangle(
				_firstDirtyRect.Value.X,
				_firstDirtyRect.Value.Y,
				_firstDirtyRect.Value.Width,
				_firstDirtyRect.Value.Height
			);

		if (_secondDirtyRect.HasValue)
			yield return new System.Drawing.Rectangle(
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
