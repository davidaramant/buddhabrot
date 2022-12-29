using Humanizer;
using SkiaSharp;

namespace Buddhabrot.Core.Boundary.Visualization;

public interface IBoundaryPalette
{
    SKColor Background { get; }
    SKColor InsideCircle { get; }
    
    SKColor this[PointClassification type] { get; }
    SKColor this[LookupRegionType type] { get; }
}

public abstract class BasePalette
{
    private readonly SKColor[] _classPalette;
    private readonly SKColor[] _palette;

    public SKColor this[PointClassification type] => _classPalette[(int) type];
    public SKColor this[LookupRegionType type] => _palette[(int) type];

    protected BasePalette(SKColor[] classPalette, SKColor[] palette)
    {
        _classPalette = classPalette;
        _palette = palette;
    }

    public override string ToString() => GetType().Name.Replace("Palette", string.Empty).Humanize();

    public static IReadOnlyCollection<IBoundaryPalette> AllPalettes { get; } = new IBoundaryPalette[]
    {
        PastelPalette.Instance,
        BluePalette.Instance,
    };
}

// https://coolors.co/96e2d9-4c7c80-011627-741a2f-e71d36-7f8b92-e4f8f4-caf1eb-2ec4b6-ff9f1c
public sealed class PastelPalette : BasePalette, IBoundaryPalette
{
    public static IBoundaryPalette Instance { get; } = new PastelPalette();

    private PastelPalette() : base(
        new[]
        {   
            SKColors.Black, // InSet
            SKColors.DimGray, // OutsideSet
            SKColors.SpringGreen, // InRange
        },
        new[]
        {
            SKColors.White, // Empty
            new(0xFF011627), // EmptyToBorder
            new(0xFFe71d36), // EmptyToFilament
            SKColors.Black, // BorderToEmpty
            SKColors.Purple, // BorderToFilament
            SKColors.Gray, // FilamentToEmpty
            SKColors.Aqua, // FilamentToBorder
            SKColors.Red, // MixedDif
        })
    {
    }
    public SKColor Background { get; } = new(0xFFcaf1eb);
    public SKColor InsideCircle { get; } = new(0xFFf1fcf8);
}

// https://colorkit.co/palette/212135-264aa7-02c9e0-f7fbfb-ffffff/
public sealed class BluePalette : BasePalette, IBoundaryPalette
{
    public static IBoundaryPalette Instance { get; } = new BluePalette();

    private BluePalette() : base(
        new[]
        {   
            SKColors.Black, // InSet
            SKColors.DimGray, // OutsideSet
            SKColors.SpringGreen, // InRange
        },
        new[]
        {
            SKColors.White, // Empty
            new(0xFF212135), // EmptyToBorder
            new(0xFF02c9e0), // EmptyToFilament
            SKColors.DarkRed, // BorderToEmpty
            SKColors.Purple, // BorderToFilament
            SKColors.Red, // FilamentToEmpty
            SKColors.MediumPurple, // FilamentToBorder
            SKColors.Green, // MixedDif
        })
    {
    }

    public SKColor Background { get; } = new(0xFFFFFFFF);
    public SKColor InsideCircle { get; } = new(0xFFf7fbfb);
}