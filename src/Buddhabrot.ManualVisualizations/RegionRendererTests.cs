using System.Drawing;
using System.Numerics;
using Buddhabrot.Core;
using Buddhabrot.Core.Boundary;
using Buddhabrot.Core.Boundary.Classifiers;
using Buddhabrot.Core.Boundary.Visualization;
using Buddhabrot.Core.Images;
using SkiaSharp;

namespace Buddhabrot.ManualVisualizations;

[Explicit]
public class RegionRendererTests : BaseVisualization
{
	[OneTimeSetUp]
	public void CreateDirectory() => SetUpOutputPath(nameof(RegionRendererTests));

	[Test]
	public async Task RenderRegions()
	{
		var scale = 10;
		var boundaryParameters = new BoundaryParameters(new AreaDivisions(VerticalPower: 10), MaxIterations: 100_000);
		var imageSize = new SKSizeI(1 << 10, 1 << 10);
		var palette = BluePalette.Instance;

		RegionLookup lookup = null!;

		await BoundaryCalculator.CalculateBoundaryAsync(
			boundaryParameters,
			ClassifierType.Default,
			(_, _, l) => lookup = l,
			CancellationToken.None
		);

		var areasToDraw = new List<RegionArea>();
		lookup.GetVisibleAreas(
			new SquareBoundary(0, 0, scale),
			[new Rectangle(0, 0, imageSize.Width, imageSize.Height)],
			areasToDraw
		);

		using var image = new RasterImage(imageSize.Width, imageSize.Height);

		using var canvas = new SKCanvas(image.Raw);
		canvas.Clear(SKColors.White);
		RegionRenderer.DrawRegions(canvas, palette, areasToDraw);

		SaveImage(image, "Borders");
	}

	[Test]
	public async Task RenderRegionInteriors()
	{
		var scale = 10;
		var boundaryParameters = new BoundaryParameters(new AreaDivisions(VerticalPower: 10), MaxIterations: 100_000);
		var imageSize = new SKSizeI(1 << 10, 1 << 10);
		var palette = BluePalette.Instance;

		RegionLookup lookup = null!;

		await BoundaryCalculator.CalculateBoundaryAsync(
			boundaryParameters,
			ClassifierType.Default,
			(_, _, l) => lookup = l,
			CancellationToken.None
		);

		var areasToDraw = new List<RegionArea>();
		lookup.GetVisibleAreas(
			new SquareBoundary(0, 0, scale),
			[new Rectangle(0, 0, imageSize.Width, imageSize.Height)],
			areasToDraw
		);

		using var image = new RasterImage(imageSize.Width, imageSize.Height);

		using var canvas = new SKCanvas(image.Raw);
		canvas.Clear(SKColors.White);
		RegionRenderer.DrawRegionInteriors(
			canvas,
			ViewPort.FromResolution(
				new Size(imageSize.Width, imageSize.Height),
				bottomLeft: new Complex(-2, -2),
				realMagnitude: 4
			),
			palette,
			maxIterations: boundaryParameters.MaxIterations,
			minIterations: boundaryParameters.MaxIterations / 100,
			areasToDraw
		);

		SaveImage(image, "Interiors");
	}
}
