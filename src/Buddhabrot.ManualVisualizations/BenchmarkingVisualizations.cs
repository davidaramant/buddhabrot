using Buddhabrot.Core;
using Buddhabrot.Core.Boundary;
using Buddhabrot.Core.Boundary.Visualization;
using SkiaSharp;

namespace Buddhabrot.ManualVisualizations;

[Explicit]
public class BenchmarkingVisualizations : BaseVisualization
{
	[OneTimeSetUp]
	public void CreateOutputPath() => SetUpOutputPath(nameof(BenchmarkingVisualizations));

	[Test]
	public void RenderRegion()
	{
		using var img = BoundaryVisualizer.RenderBorderRegion(
			new SKSizeI(1000, 1000),
			new AreaDivisions(19),
			new RegionId(X: 170_725, Y: 16_565),
			new IterationRange(Min: 100_000, Max: 1_000_000)
		);
		SaveImage(img, "Test Region");
	}
}
