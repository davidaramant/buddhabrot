using Buddhabrot.Core.Utilities;

namespace Buddhabrot.Core.Tests.Utilities;

public class BoolVector16Tests
{
	[Fact]
	public void ShouldGetIfBitSet()
	{
		var v = BoolVector16.Empty.WithBit(0).WithBit(2);

		v[0].ShouldBe(1);
		v[1].ShouldBe(0);
		v[2].ShouldBe(1);
	}
}
