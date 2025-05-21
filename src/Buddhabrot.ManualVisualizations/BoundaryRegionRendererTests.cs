using Buddhabrot.Core.Boundary;
using Buddhabrot.Core.Boundary.Classifiers;
using Buddhabrot.Core.Boundary.Visualization;
using Buddhabrot.Core.Images;
using SkiaSharp;

namespace Buddhabrot.ManualVisualizations;

[Explicit]
public class BoundaryRegionRendererTests : BaseVisualization
{
	[OneTimeSetUp]
	public void CreateDirectory() => SetUpOutputPath(nameof(BoundaryRegionRendererTests));

	[Test]
	public async Task RenderRegionAreas()
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
			new QuadtreeViewport(SKRectI.Create(new SKPointI(0, 0), imageSize), scale),
			[SKRectI.Create(0, 0, imageSize.Width, imageSize.Height)],
			areasToDraw
		);

		using var image = new RasterImage(imageSize.Width, imageSize.Height);

		using var canvas = new SKCanvas(image.Raw);
		canvas.Clear(SKColors.White);
		BoundaryRegionRenderer.DrawRegionAreas(canvas, palette, areasToDraw);

		SaveImage(image, "Region Areas");
	}

	[Test]
	public async Task RenderRegionInteriors()
	{
		var boundaryParameters = new BoundaryParameters(new AreaDivisions(VerticalPower: 10), MaxIterations: 100_000);
		var imageSize = new SKSizeI(1024, 1024);

		RegionLookup lookup = null!;

		await BoundaryCalculator.CalculateBoundaryAsync(
			boundaryParameters,
			ClassifierType.Default,
			(_, _, l) => lookup = l,
			CancellationToken.None
		);

		var args = new RenderingArgs(
			Instructions: RenderInstructions.Everything(imageSize),
			Lookup: lookup,
			Palette: BluePalette.Instance,
			RenderInteriors: true,
			MaxIterations: boundaryParameters.MaxIterations,
			MinIterations: 0
		);

		var areasToDraw = new List<RegionArea>();
		lookup.GetVisibleAreas(
			args.Instructions.QuadtreeViewport,
			[SKRectI.Create(0, 0, imageSize.Width, imageSize.Height)],
			areasToDraw
		);

		using var image = new RasterImage(imageSize.Width, imageSize.Height);

		using var canvas = new SKCanvas(image.Raw);
		canvas.Clear(SKColors.White);
		BoundaryRegionRenderer.DrawRegionInteriors(
			canvas,
			args.Instructions.ComplexViewport,
			args.Palette,
			maxIterations: args.MaxIterations,
			minIterations: args.MinIterations,
			areasToDraw
		);

		SaveImage(image, "Region Interiors");
	}

	[Test]
	public async Task RenderRegion()
	{
		var boundaryParameters = new BoundaryParameters(new AreaDivisions(VerticalPower: 10), MaxIterations: 100_000);
		var imageSize = new SKSizeI(1024, 1024);

		RegionLookup lookup = null!;

		await BoundaryCalculator.CalculateBoundaryAsync(
			boundaryParameters,
			ClassifierType.Default,
			(_, _, l) => lookup = l,
			CancellationToken.None
		);

		var args = new RenderingArgs(
			Instructions: RenderInstructions.Everything(imageSize),
			Lookup: lookup,
			Palette: BluePalette.Instance,
			RenderInteriors: true,
			MaxIterations: boundaryParameters.MaxIterations,
			MinIterations: 0
		);

		var areasToDraw = new List<RegionArea>();
		lookup.GetVisibleAreas(
			args.Instructions.QuadtreeViewport,
			[SKRectI.Create(0, 0, imageSize.Width, imageSize.Height)],
			areasToDraw
		);

		using var image = new RasterImage(imageSize.Width, imageSize.Height);

		using var canvas = new SKCanvas(image.Raw);
		canvas.Clear(SKColors.White);
		BoundaryRegionRenderer.DrawRegions(args, canvas, image.Raw);

		SaveImage(image, "Region");
	}
}
