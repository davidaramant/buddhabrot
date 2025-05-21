using Buddhabrot.Core.Boundary;
using Buddhabrot.Core.Boundary.Quadtrees;

namespace Buddhabrot.Core.Tests.Boundary.Quadtrees;

public class RegionNodeTests
{
	public static IEnumerable<object[]> AllRegionTypes()
	{
		foreach (var entry in Enum.GetValues<LookupRegionType>())
		{
			yield return [entry];
		}
	}

	[Theory]
	[MemberData(nameof(AllRegionTypes))]
	public void ShouldConstructLeafCorrectly(LookupRegionType type)
	{
		var node = RegionNode.MakeLeaf(type);

		node.IsLeaf.ShouldBeTrue();
		node.RegionType.ShouldBe(type);
	}

	[Theory]
	[MemberData(nameof(AllRegionTypes))]
	public void ShouldConstructBranchCorrectly(LookupRegionType type)
	{
		var node = RegionNode.MakeBranch(type, 123_456);

		node.IsLeaf.ShouldBeFalse();
		node.RegionType.ShouldBe(type);

		node.ChildIndex.ShouldBe(123_456);
	}
}
