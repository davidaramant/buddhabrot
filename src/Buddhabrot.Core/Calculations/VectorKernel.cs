using System.Numerics;
using CommunityToolkit.HighPerformance.Helpers;

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
			ParallelHelper.For(0, numPoints, new EscapeTimeAction(points, escapeTimes, maxIterations));
		}
		catch (OperationCanceledException)
		{
			// do nothing
		}
	}

	private readonly struct EscapeTimeAction(Complex[] points, EscapeTime[] escapeTimes, int maxIterations) : IAction
	{
		public void Invoke(int i)
		{
			escapeTimes[i] = ScalarKernel.FindEscapeTime(points[i], maxIterations);
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
			ParallelHelper.For(0, numPoints, new DemAction(points, escapeTimes, distances, maxIterations));
		}
		catch (OperationCanceledException)
		{
			// do nothing
		}
	}

	private readonly struct DemAction(Complex[] points, EscapeTime[] escapeTimes, double[] distances, int maxIterations)
		: IAction
	{
		public void Invoke(int i)
		{
			var (escapeTime, distance) = ScalarKernel.FindExteriorDistance(points[i], maxIterations);
			escapeTimes[i] = escapeTime;
			distances[i] = distance;
		}
	}
}
