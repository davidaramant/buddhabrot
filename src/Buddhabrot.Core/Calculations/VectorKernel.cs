using System.Numerics;

namespace Buddhabrot.Core.Calculations;

public static class VectorKernel
{
	public static void FindEscapeTimes(
		Complex[] points,
		EscapeTime[] escapeTimes,
		int numPoints,
		int maxIterations,
		CancellationToken cancelToken = default
	)
	{
		try
		{
			Parallel.For(
				0,
				numPoints,
				new ParallelOptions { CancellationToken = cancelToken },
				i => escapeTimes[i] = ScalarKernel.FindEscapeTime(points[i], maxIterations)
			);
		}
		catch (OperationCanceledException)
		{
			// do nothing
		}
	}
}
