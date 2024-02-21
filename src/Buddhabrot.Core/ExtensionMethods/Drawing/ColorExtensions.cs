using SkiaSharp;

namespace Buddhabrot.Core.ExtensionMethods.Drawing;

public static class ColorExtensions
{
    public static SKColor ToSKColor(this System.Drawing.Color color) =>
        new(red: color.R, green: color.G, blue: color.B);
}
