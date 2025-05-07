using System.Drawing;
using Buddhabrot.Core;
using Buddhabrot.Core.Boundary;
using Buddhabrot.Core.Calculations;
using Buddhabrot.ManualVisualizations;
using Humanizer;
using SkiaSharp;

namespace Buddhabrot.Benchmarks;

public static class RenderRegions
{
	private static readonly string OutputPath = DataLocation
		.CreateDirectory("Benchmarks", nameof(RenderRegions).Humanize())
		.FullName;

	private static readonly IReadOnlyCollection<(int VP, int X, int Y)> Regions = [(2, 1, 0), (19, 209_621, 40_358)];

	private const int MaxIterations = 100_000;

	public static void Run()
	{
		foreach (var (vp, x, y) in Regions)
		{
			var divisions = new AreaDivisions(VerticalPower: vp);
			var regionId = new RegionId(x, y);
			var resolution = new Size(256, 256);
			var viewPort = ComplexViewport.FromRegionId(resolution, divisions, regionId);

			using var bitmap = new SKBitmap(viewPort.Resolution.Width, viewPort.Resolution.Height);

			DrawContents(bitmap, viewPort);

			var fileName = $"{vp}.{x}.{y}.png";
			var filePath = Path.Combine(OutputPath, fileName);
			using var stream = File.Open(filePath, FileMode.Create);
			bitmap.Encode(stream, SKEncodedImageFormat.Png, quality: 100);
		}
	}

	private static void DrawContents(SKBitmap bitmap, ComplexViewport viewPort)
	{
		for (var y = 0; y < viewPort.Resolution.Height; y++)
		{
			for (var x = 0; x < viewPort.Resolution.Width; x++)
			{
				var c = viewPort.GetComplex(x, y);
				var isInSet = ScalarKernel.FindEscapeTime(c, maxIterations: MaxIterations).IsInfinite;
				bitmap.SetPixel(x, y, isInSet ? SKColors.Black : SKColors.White);
			}
		}
	}
}
