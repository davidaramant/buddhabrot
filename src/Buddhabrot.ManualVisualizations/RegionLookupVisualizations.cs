using Buddhabrot.Core.Boundary.Visualization;
using Buddhabrot.Core.Tests.Boundary.Quadtrees;

namespace Buddhabrot.ManualVisualizations;

[Explicit]
public class RegionLookupVisualizations : BaseVisualization
{
	[Before(Class)]
	public static void SetOutputPath() => SetUpOutputPath(nameof(RegionLookupVisualizations));

	[Test]
	public void ShouldConstructPower2QuadtreeCorrectly()
	{
		var lookup = RegionLookupUtil.Power2Lookup;

		using var image = BoundaryVisualizer.RenderRegionLookup(lookup);
		SaveImage(image, "Power 2 Quadtree");
	}

	[Test]
	public void ShouldConstructPower3QuadtreeCorrectly()
	{
		var lookup = RegionLookupUtil.Power3Lookup;

		using var image = BoundaryVisualizer.RenderRegionLookup(lookup);
		SaveImage(image, "Power 3 Quadtree");
	}

	[Test]
	[Arguments(0, 0)]
	[Arguments(0, 1)]
	[Arguments(1, 1)]
	[Arguments(1, 0)]
	[Arguments(2, 0)]
	[Arguments(2, 1)]
	[Arguments(3, 1)]
	[Arguments(3, 0)]
	public void ShouldRenderSimpleQuadtreeWithBorder(int x, int y)
	{
		var lookup = RegionLookupUtil.Make((x, y));
		using var image = BoundaryVisualizer.RenderRegionLookup(lookup);
		SaveImage(image, $"Border {x} {y}");
	}
}
