using System.Collections;
using System.Collections.Generic;
using Avalonia;

namespace BoundaryFinder.Views;

public sealed class RenderInstructions : IEnumerable<Rect>
{
    private readonly Rect? _firstDirtyRect;
    private readonly Rect? _secondDirtyRect;

    public bool PasteFrontBuffer { get; }

    private RenderInstructions(bool pasteFrontBuffer, Rect? firstDirtyRect, Rect? secondDirtyRect)
    {
        PasteFrontBuffer = pasteFrontBuffer;
        _firstDirtyRect = firstDirtyRect;
        _secondDirtyRect = secondDirtyRect;
    }

    public static RenderInstructions Everything(Size newSize) =>
        new(
            pasteFrontBuffer: false,
            firstDirtyRect: new Rect(new Point(0, 0), newSize),
            secondDirtyRect: null);

    public static RenderInstructions Calculate(Size oldSize, Size newSize)
    {
        if (newSize.Width <= oldSize.Width && newSize.Height <= oldSize.Height)
        {
            return new RenderInstructions(
                pasteFrontBuffer: true,
                firstDirtyRect: null,
                secondDirtyRect: null);
        }

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
            pasteFrontBuffer: true,
            firstDirtyRect: horizontal,
            secondDirtyRect: vertical);
    }

    public IEnumerator<Rect> GetEnumerator()
    {
        if (_firstDirtyRect.HasValue)
            yield return _firstDirtyRect.Value;

        if (_secondDirtyRect.HasValue)
            yield return _secondDirtyRect.Value;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}