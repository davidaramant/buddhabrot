using Buddhabrot.Core.Boundary;
using Buddhabrot.Core.Boundary.Quadtrees;

namespace Buddhabrot.Core.Tests.Boundary.Quadtrees;

public class VisitedRegionsToRegionLookupTests
{
	[Theory]
	[InlineData(
		LookupRegionType.EmptyToBorder,
		LookupRegionType.Empty,
		LookupRegionType.Empty,
		LookupRegionType.Empty,
		LookupRegionType.EmptyToBorder
	)]
	[InlineData(
		LookupRegionType.EmptyToBorder,
		LookupRegionType.EmptyToFilament,
		LookupRegionType.EmptyToFilament,
		LookupRegionType.Empty,
		LookupRegionType.EmptyToFilament
	)]
	public void ShouldCondenseRegionTypes(
		LookupRegionType ll,
		LookupRegionType lr,
		LookupRegionType ul,
		LookupRegionType ur,
		LookupRegionType expected
	)
	{
		QuadtreeCompressor.CondenseRegionType(ll, lr, ul, ur).ShouldBe(expected);
	}

	[Fact]
	public void ShouldCondenseIdenticalLeaves()
	{
		var transformer = new QuadtreeCompressor(new VisitedRegions());
		var node = transformer.MakeQuad(RegionNode.Empty, RegionNode.Empty, RegionNode.Empty, RegionNode.Empty);

		node.ShouldBe(RegionNode.Empty);
	}
}
