namespace Buddhabrot.Core;

public static class Utility
{
	public static int GetLargestPowerOfTwoLessThan(int max)
	{
		int pow = 0;

		while ((2 << pow) <= max)
		{
			pow++;
		}

		return pow;
	}
}
