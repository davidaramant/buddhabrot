namespace Buddhabrot.ManualVisualizations;

[Explicit]
public class RegionRendererTests : BaseVisualization
{
	[OneTimeSetUp]
	public void CreateDirectory() => SetUpOutputPath(nameof(RegionRendererTests));

	[Test]
	public void RenderRegions() { }
}
