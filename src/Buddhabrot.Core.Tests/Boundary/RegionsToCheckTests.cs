using Buddhabrot.Core.Boundary;

namespace Buddhabrot.Core.Tests.Boundary;

public class RegionsToCheckTests
{
	[Fact]
	public void ShouldAllowAddingMoreItemsWhileIteratingOver()
	{
		RegionsToCheck regions = [new RegionId(0, 0)];

		var iterated = new List<RegionId>();

		foreach (var region in regions)
		{
			iterated.Add(region);

			if (iterated.Count < 4)
			{
				regions.Add(new RegionId(iterated.Count, iterated.Count));
			}
		}

		iterated.ShouldBe(new RegionId[] { new(0, 0), new(1, 1), new(2, 2), new(3, 3) });
	}
}
