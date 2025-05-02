using Buddhabrot.Core.Boundary;
using Buddhabrot.Core.Boundary.Visualization;

namespace Buddhabrot.Core.Tests.Boundary.Visualization;

public class LookupRegionTypeListTests
{
	[Fact]
	public void ShouldReturnCorrectTypesForMultipleRuns()
	{
		var list = new LookupRegionTypeList();
		list.Add(LookupRegionType.MixedDiff, 2);
		list.Add(LookupRegionType.MixedDiff, 3);

		for (int i = 0; i < 5; i++)
		{
			list.GetNextType().ShouldBe(LookupRegionType.MixedDiff);
		}
	}

	[Fact]
	public void ShouldReturnCorrectTypesForMultipleTypes()
	{
		var list = new LookupRegionTypeList();
		list.Add(LookupRegionType.MixedDiff, 2);
		list.Add(LookupRegionType.EmptyToBorder, 3);

		for (int i = 0; i < 2; i++)
		{
			list.GetNextType().ShouldBe(LookupRegionType.MixedDiff);
		}
		for (int i = 0; i < 3; i++)
		{
			list.GetNextType().ShouldBe(LookupRegionType.EmptyToBorder);
		}
	}
}
