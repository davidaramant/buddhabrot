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
        var types = Enum.GetValues<LookupRegionType>()
            .Where(t => t != LookupRegionType.Empty)
            .ToArray();

        using var image = new RasterImage(3, types.Length, scale: 100);

        foreach (IBoundaryPalette palette in BasePalette.AllPalettes)
        {
            for (int i = 0; i < types.Length; i++)
            {
                var c = palette[types[i]];

                c.ToHsv(out var h, out var s, out var v);

                // InsideSet
                image.SetPixel(0, i, c);

                // InRange
                image.SetPixel(1, i, SKColor.FromHsv((h + 180f) % 360f, 100, 100));
                
                // OutsideSet
                image.SetPixel(2, i, SKColor.FromHsv(h, 20, 100));
            }

            SaveImage(image, palette.ToString()!);
        }
    }
}