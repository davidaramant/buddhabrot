using System.Diagnostics;
using BenchmarkDotNet.Running;
using Buddhabrot.Benchmarks;
using Humanizer;

static void SimpleBenchmark<T>(Func<T> method, string name)
{
	Console.WriteLine($"{new string('-', 20)}\n{name}\nResult: {method()}");
	GC.Collect();
	const int trials = 3;
	var timer = Stopwatch.StartNew();

	for (int i = 0; i < trials; i++)
	{
		GC.KeepAlive(method());
	}

	timer.Stop();
	Console.WriteLine(timer.Elapsed.Humanize(2));
}

// TODO: Commenting these out is a nightmare since stuff will break
// Make an enum or something about which ones to run. Put in a list and loop over it.

VisitedRegionsDataSet.Create();
BenchmarkRunner.Run<VisitedRegionsBenchmark>();

BenchmarkRunner.Run<VisitNodeWithQuadrantBenchmarks>();
BenchmarkRunner.Run<VisitNodeGetQuadrantBenchmarks>();

BenchmarkRunner.Run<QuadDimensionDetermineQuadrantBenchmarks>();
BenchmarkRunner.Run<QuadDimensionGetQuadrantBenchmarks>();

BenchmarkRunner.Run<FixedSizeCacheBenchmarks>();

CoordinateHashingTests.ComputeHistograms();
BenchmarkRunner.Run<CoordinateHashingBenchmarks>();
BenchmarkRunner.Run<SquareBoundaryIntersectionBenchmarks>();
