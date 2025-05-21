using Buddhabrot.Core.Boundary;
using Buddhabrot.Core.Boundary.Quadtrees;
using Buddhabrot.Core.Tests.Boundary.Quadtrees;
using SkiaSharp;

namespace Buddhabrot.Core.Tests.Boundary;

public class RegionLookupTests
{
	[Fact]
	public void ShouldReturnEveryAreaPlusMirrorsIfSearchAreaIsEqualToBounds()
	{
		var lookup = RegionLookupUtil.Make((0, 0), (4, 1));
		var visibleAreas = new List<RegionArea>();

		lookup.GetVisibleAreas(
			new QuadtreeViewport(SKRectI.Create(new SKPointI(0, 0), new SKSizeI(100, 100)), 2),
			[SKRectI.Create(0, 0, 8, 8)],
			visibleAreas
		);

		visibleAreas.Count.ShouldBe(4);
	}

	[Fact]
	public void ShouldReturnIntersectionsWithSearchArea()
	{
		var lookup = new RegionLookup(
			[
				RegionNode.Empty,
				RegionNode.Empty,
				RegionNode.MakeLeaf(LookupRegionType.EmptyToBorder),
				RegionNode.Empty,
				RegionNode.Empty,
				RegionNode.Empty,
				RegionNode.MakeBranch(LookupRegionType.EmptyToBorder, 0),
				RegionNode.Empty,
				RegionNode.MakeBranch(LookupRegionType.EmptyToBorder, 4),
			],
			2
		);
		var visibleAreas = new List<RegionArea>();

		lookup.GetVisibleAreas(
			new QuadtreeViewport(SKRectI.Create(new SKPointI(0, 0), new SKSizeI(100, 100)), 2),
			[SKRectI.Create(0, 0, 1, 1)],
			visibleAreas
		);

		visibleAreas.Count.ShouldBe(1);
	}
}
