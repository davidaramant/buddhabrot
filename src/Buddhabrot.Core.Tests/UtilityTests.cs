namespace Buddhabrot.Core.Tests;

public class UtilityTests
{
	[Theory]
	[InlineData(2, 1)]
	[InlineData(3, 1)]
	[InlineData(4, 2)]
	[InlineData(5, 2)]
	[InlineData(257, 8)]
	public void ShouldCalculatePowerOfTwo(int max, int expectedPow)
	{
		Utility.GetLargestPowerOfTwoLessThan(max).ShouldBe(expectedPow);
	}
}
