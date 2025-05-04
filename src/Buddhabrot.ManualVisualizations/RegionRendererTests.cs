using System.Drawing;
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

		IReadOnlyList<RegionId> borders = null!;
		RegionLookup lookup = null!;

		await BoundaryCalculator.CalculateBoundaryAsync(
			boundaryParameters,
			ClassifierType.Default,
			(_, b, l) =>
			{
				borders = b;
				lookup = l;
			},
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
}
