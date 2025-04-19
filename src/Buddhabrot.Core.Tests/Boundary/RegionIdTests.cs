using Buddhabrot.Core.Boundary;

namespace Buddhabrot.Core.Tests.Boundary;

public class RegionIdTests
{
	[Fact]
	public void ShouldHaveCorrectXAndY()
	{
		var id = RegionId.FromEncodedPosition((3 << 16) + 2);
		id.X.ShouldBe(2);
		id.Y.ShouldBe(3);
	}

	[Fact]
	public void ShouldCreateEquivalentIdFromXAndY()
	{
		var id1 = RegionId.FromEncodedPosition((3 << 16) + 2);
		var id2 = new RegionId(2, 3);
		id1.ShouldBe(id2);
	}
}
