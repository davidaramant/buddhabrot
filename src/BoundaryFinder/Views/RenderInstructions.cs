using System;
using System.Collections.Generic;
using Avalonia;

namespace BoundaryFinder.Views;

// TODO: This needs to expose SoureRect and DestRect for pasting images
public sealed class RenderInstructions
{
    private readonly Rect? _firstDirtyRect;
    private readonly Rect? _secondDirtyRect;

    public Point? PasteOffset { get; }
    public bool PasteFrontBuffer => PasteOffset.HasValue;

    private RenderInstructions(Point? pasteOffset, Rect? firstDirtyRect, Rect? secondDirtyRect)
    {
        PasteOffset = pasteOffset;
        _firstDirtyRect = firstDirtyRect;
        _secondDirtyRect = secondDirtyRect;
    }

    public static RenderInstructions Everything(Size newSize) =>
        new(
            pasteOffset: null,
            firstDirtyRect: new Rect(new Point(0, 0), newSize),
            secondDirtyRect: null);

    public static RenderInstructions Resized(Size oldSize, Size newSize)
    {
        Rect? horizontal = null;
        if (newSize.Width > oldSize.Width)
        {
            horizontal = new Rect(
                x: oldSize.Width,
                y: 0,
                width: newSize.Width - oldSize.Width,
                height: newSize.Height);
        }

        Rect? vertical = null;
        if (newSize.Height > oldSize.Height)
        {
            vertical = new Rect(
                x: 0,
                y: oldSize.Height,
                width: oldSize.Width,
                height: newSize.Height - oldSize.Height);
        }

        return new RenderInstructions(
            pasteOffset: new Point(0, 0),
            firstDirtyRect: horizontal,
            secondDirtyRect: vertical);
    }

    public static RenderInstructions Moved(Size size, Point offset)
    {
        Rect? horizontal = null;
        if (offset.X != 0)
        {
            if (offset.X < 0)
            {
                horizontal = new Rect(
                    size.Width + offset.X,
                    0,
                    width: Math.Abs(offset.X),
                    size.Height);
            }
            else
            {
                horizontal = new Rect(
                    0,
                    0,
                    width: offset.X,
                    size.Height);
            }
        }

        Rect? vertical = null;
        if (offset.Y != 0)
        {
            if (offset.Y < 0) // Up
            {
                vertical = new Rect(
                    x: Math.Max(0, offset.X),
                    y: size.Height + offset.Y,
                    width: Math.Min(size.Width, size.Width - Math.Abs(offset.X)),
                    height: Math.Abs(offset.Y));
            }
            else // Down
            {
                vertical = new Rect(
                    x: Math.Max(0, offset.X),
                    y: 0,
                    width: Math.Min(size.Width, size.Width - Math.Abs(offset.X)),
                    height: offset.Y);
            }
        }

        return new RenderInstructions(
            pasteOffset: offset,
            firstDirtyRect: horizontal,
            secondDirtyRect: vertical);
    }

    public IEnumerable<Rect> GetDirtyRectangles()
    {
        if (_firstDirtyRect.HasValue)
            yield return _firstDirtyRect.Value;

        if (_secondDirtyRect.HasValue)
            yield return _secondDirtyRect.Value;
    }
}