namespace Buddhabrot.Core.Tests;

public class UtilityTests
{
	[Test]
	[Arguments(2, 1)]
	[Arguments(3, 1)]
	[Arguments(4, 2)]
	[Arguments(5, 2)]
	[Arguments(257, 8)]
	public void ShouldCalculatePowerOfTwo(int max, int expectedPow)
	{
		Utility.GetLargestPowerOfTwoLessThan(max).ShouldBe(expectedPow);
	}
}