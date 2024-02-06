using System.Drawing;
using Buddhabrot.Core.Boundary;
using Buddhabrot.Core.Boundary.QuadTrees;
using Buddhabrot.Core.Tests.Boundary.QuadTrees;

namespace Buddhabrot.Core.Tests.Boundary;

public class RegionLookupTests
{
	[Fact]
	public void ShouldReturnEveryAreaPlusMirrorsIfSearchAreaIsEqualToBounds()
	{
		var lookup = RegionLookupUtil.Make((0, 0), (4, 1));
		var visibleAreas = new List<(Rectangle, LookupRegionType)>();

		lookup.GetVisibleAreas(new SquareBoundary(0, 0, 2), new[] { new Rectangle(0, 0, 8, 8) }, visibleAreas);

		visibleAreas.Should().HaveCount(4);
	}

	[Fact]
	public void ShouldReturnIntersectionsWithSearchArea()
	{
		var lookup = new RegionLookup(
			new[]
			{
				RegionNode.Empty,
				RegionNode.Empty,
				RegionNode.MakeLeaf(LookupRegionType.EmptyToBorder),
				RegionNode.Empty,
				RegionNode.Empty,
				RegionNode.Empty,
				RegionNode.MakeBranch(LookupRegionType.EmptyToBorder, 0),
				RegionNode.Empty,
				RegionNode.MakeBranch(LookupRegionType.EmptyToBorder, 4),
			},
			2
		);
		var visibleAreas = new List<(Rectangle, LookupRegionType)>();

		lookup.GetVisibleAreas(new SquareBoundary(0, 0, 2), new[] { new Rectangle(0, 0, 1, 1) }, visibleAreas);

		visibleAreas.Should().HaveCount(1);
	}
}
