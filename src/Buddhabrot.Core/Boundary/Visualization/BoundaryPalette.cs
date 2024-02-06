using Colourful;
using Humanizer;
using SkiaSharp;

namespace Buddhabrot.Core.Boundary.Visualization;

public interface IBoundaryPalette
{
	SKColor Background { get; }
	SKColor InsideCircle { get; }

	SKColor this[LookupRegionType type, PointClassification classification] { get; }
	SKColor this[LookupRegionType type] { get; }

	public static IReadOnlyCollection<IBoundaryPalette> AllPalettes { get; } =
		new[]
		{
			PastelPalette.Instance,
			BluePalette.Instance,
			HsvRainbowPalette.Instance,
			LChuvRainbowPalette.Instance,
			BlackAndWhitePalette.Instance,
		};
}

public abstract class BasePalette
{
	private readonly SKColor[] _palette;

	public SKColor this[LookupRegionType type, PointClassification classification] =>
		_palette[((int)type) * 3 + (int)classification];

	public SKColor this[LookupRegionType type] => _palette[((int)type) * 3];

	protected BasePalette(SKColor[] palette) => _palette = palette;

	public override string ToString() => GetType().Name.Replace("Palette", string.Empty).Humanize();
}

public abstract class ComputedPalette : BasePalette
{
	protected ComputedPalette(IEnumerable<SKColor> palette)
		: base(
			palette
				.SelectMany(c =>
				{
					var variants = new SKColor[3];
					// InSet
					variants[0] = c;

					c.ToHsv(out var h, out var s, out var v);

					// OutsideSet
					variants[1] = SKColor.FromHsv(h, 20, 100);

					// InRange
					variants[2] = SKColor.FromHsv((h + 180f) % 360f, 100, 100);

					return variants;
				})
				.ToArray()
		) { }
}

// https://coolors.co/96e2d9-4c7c80-011627-741a2f-e71d36-7f8b92-e4f8f4-caf1eb-2ec4b6-ff9f1c
public sealed class PastelPalette : ComputedPalette, IBoundaryPalette
{
	public static IBoundaryPalette Instance { get; } = new PastelPalette();

	private PastelPalette()
		: base(
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
			}
		) { }

	public SKColor Background { get; } = new(0xFFcaf1eb);
	public SKColor InsideCircle { get; } = new(0xFFf1fcf8);
}

// https://colorkit.co/palette/212135-264aa7-02c9e0-f7fbfb-ffffff/
public sealed class BluePalette : ComputedPalette, IBoundaryPalette
{
	public static IBoundaryPalette Instance { get; } = new BluePalette();

	private BluePalette()
		: base(
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
			}
		) { }

	public SKColor Background { get; } = new(0xFFFFFFFF);
	public SKColor InsideCircle { get; } = new(0xFFf7fbfb);
}

public sealed class HsvRainbowPalette : ComputedPalette, IBoundaryPalette
{
	public static IBoundaryPalette Instance { get; } = new HsvRainbowPalette();

	private HsvRainbowPalette()
		: base(
			new[]
			{
				SKColors.White, // Empty
				SKColor.FromHsv(0f / 7f * 360f, 100, 100), // EmptyToBorder
				SKColor.FromHsv(1f / 7f * 360f, 100, 100), // EmptyToFilament
				SKColor.FromHsv(2f / 7f * 360f, 100, 100), // BorderToEmpty
				SKColor.FromHsv(3f / 7f * 360f, 100, 100), // BorderToFilament
				SKColor.FromHsv(4f / 7f * 360f, 100, 100), // FilamentToEmpty
				SKColor.FromHsv(5f / 7f * 360f, 100, 100), // FilamentToBorder
				SKColor.FromHsv(6f / 7f * 360f, 100, 100), // MixedDif
			}
		) { }

	public SKColor Background { get; } = new(0xFFFFFFFF);
	public SKColor InsideCircle { get; } = new(0xFFf7fbfb);
}

public sealed class LChuvRainbowPalette : BasePalette, IBoundaryPalette
{
	public static IBoundaryPalette Instance { get; } = new LChuvRainbowPalette();

	private LChuvRainbowPalette()
		: base(CreatePalette().ToArray()) { }

	public override string ToString() => "LChuv Rainbow";

	public SKColor Background { get; } = new(0xFFFFFFFF);
	public SKColor InsideCircle { get; } = new(0xFFf7fbfb);

	private static IEnumerable<SKColor> CreatePalette()
	{
		yield return SKColors.White;
		yield return SKColors.White;
		yield return SKColors.White;

		var rgbWorkingSpace = RGBWorkingSpaces.sRGB;

		var lChuvToRgb = new ConverterBuilder().FromLChuv(rgbWorkingSpace.WhitePoint).ToRGB(rgbWorkingSpace).Build();

		for (int i = 0; i < 7; i++)
		{
			double l = 100;
			double c = 100;
			double h = i / 7d * 360;
			yield return Convert(new LChuvColor(l, c, h));

			// Outside set
			yield return Convert(new LChuvColor(l, 50, h));

			// In Range
			yield return Convert(new LChuvColor(l, c, (h + 180) % 360));
		}

		SKColor Convert(LChuvColor color)
		{
			var rgbColor = lChuvToRgb.Convert(color);
			return new((byte)(rgbColor.R * 255), (byte)(rgbColor.G * 255), (byte)(rgbColor.B * 255));
		}
	}
}

public sealed class BlackAndWhitePalette : BasePalette, IBoundaryPalette
{
	public static IBoundaryPalette Instance { get; } = new BlackAndWhitePalette();

	private BlackAndWhitePalette()
		: base(
			Enumerable
				.Repeat(new[] { SKColors.Black, SKColors.LightGray, SKColors.Gold }, 8)
				.SelectMany(c => c)
				.ToArray()
		) { }

	public SKColor Background { get; } = new(0xFFFFFFFF);
	public SKColor InsideCircle { get; } = new(0xFFfcfcfc);
}
