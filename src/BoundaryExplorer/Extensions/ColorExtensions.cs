using Avalonia.Media;
using SkiaSharp;

namespace BoundaryExplorer.Extensions;

public static class ColorExtensions
{
    public static Color ToAvaloniaColor(this SKColor skColor) =>
        Color.FromRgb(skColor.Red, skColor.Green, skColor.Blue);
}
