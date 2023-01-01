using Buddhabrot.Core.Boundary;
using Buddhabrot.Core.Boundary.Visualization;
using Buddhabrot.Core.Images;
using SkiaSharp;

namespace Buddhabrot.ManualVisualizations;

[Explicit]
public class ColorVariations : BaseVisualization
{
    [OneTimeSetUp]
    public void CreateOutputPath() => SetUpOutputPath(nameof(ColorVariations));

    [Test]
    public void CreateColorVariations()
    {
        var palette = BluePalette.Instance;

        var types = Enum.GetValues<LookupRegionType>();

        using var image = new RasterImage(3, types.Length, scale: 40);

        for (int i = 0; i < types.Length; i++)
        {
            var c = palette[types[i]];

            c.ToHsv(out var h, out var s, out var v);

            // InsideSet
            image.SetPixel(0, i, c);

            // OutsideSet
            image.SetPixel(1, i, SKColor.FromHsv(h, 20, 100));

            // InRange
            image.SetPixel(2, i, SKColor.FromHsv((h + 180f) % 360f, 100, 100));
        }

        static double Luminance(SKColor c) =>
            (0.2126 * c.Red) + (0.7152 * c.Green) + (0.0722 * c.Blue); // SMPTE C, Rec. 709 weightings

        SaveImage(image, "color variants");
    }
}