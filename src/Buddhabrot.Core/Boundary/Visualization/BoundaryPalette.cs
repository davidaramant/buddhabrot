using Humanizer;
using SkiaSharp;

namespace Buddhabrot.Core.Boundary.Visualization;

public interface IBoundaryPalette
{
    SKColor Background { get; }
    SKColor InBounds { get; }
    
    // TODO: These should be based on a modifier or something, since "Border" is more than one color now
    SKColor BorderInSet { get; }
    SKColor BorderInRange { get; }
    SKColor BorderEmpty { get; }
    
    SKColor this[LookupRegionType type] { get; }
}

public abstract class BasePalette
{
    private readonly SKColor[] _palette;

    public SKColor this[LookupRegionType type] => _palette[(int) type];

    protected BasePalette(SKColor[] palette) => _palette = palette;

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

    public SKColor Visited { get; } = new(0xFFE4F8F4);
    public SKColor Background { get; } = new(0xFFcaf1eb);
    public SKColor InBounds { get; } = new(0xFFf1fcf8);
    public SKColor InSet { get; } = new(0xFF96e2d9);
    public SKColor Border { get; } = new(0xFF011627);
    public SKColor BorderInSet { get; } = new(0xFF4c7c80);
    public SKColor BorderInRange { get; } = new(0xFFff9f1c);
    public SKColor BorderEmpty { get; } = new(0xFF741a2f);
    public SKColor Filament { get; } = new(0xFFe71d36);
}

// https://colorkit.co/palette/212135-264aa7-02c9e0-f7fbfb-ffffff/
public sealed class BluePalette : BasePalette, IBoundaryPalette
{
    public static IBoundaryPalette Instance { get; } = new BluePalette();

    private BluePalette() : base(
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

    public SKColor Border { get; } = new(0xFF212135);
    public SKColor Background { get; } = new(0xFFFFFFFF);
    public SKColor InBounds { get; } = new(0xFFf7fbfb);
    public SKColor Filament { get; } = new(0xFF02c9e0);
    public SKColor BorderEmpty { get; } = new(0xFF264aa7);
    public SKColor BorderInSet => Border;


    // Not updated
    public SKColor Visited { get; } = new(0xFFE4F8F4);
    public SKColor InSet { get; } = new(0xFF96e2d9);
    public SKColor BorderInRange { get; } = new(0xFFff9f1c);
}