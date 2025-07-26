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

	public static void FindDistances(
		Complex[] points,
		EscapeTime[] escapeTimes,
		double[] distances,
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
				i =>
				{
					var (escapeTime, distance) = ScalarKernel.FindExteriorDistance(points[i], maxIterations);
					escapeTimes[i] = escapeTime;
					distances[i] = distance;
				}
			);
		}
		catch (OperationCanceledException)
		{
			// do nothing
		}
	}
}
