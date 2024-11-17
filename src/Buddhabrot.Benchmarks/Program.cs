using BenchmarkDotNet.Running;
using Buddhabrot.Benchmarks;

// ReSharper disable InconsistentNaming

var toRun = new[]
{
	// Benchmark.VisitedRegions,
	// Benchmark.VisitNode_GetQuadrant,
	// Benchmark.VisitNode_WithQuadrant,
	// Benchmark.FixedSizeCache,
	// Benchmark.CoordinateHashing,
	// Benchmark.SquareBoundaryIntersections
	Benchmark.RenderRegion,
};

foreach (var benchmark in toRun)
{
	switch (benchmark)
	{
		case Benchmark.VisitedRegions:
			VisitedRegionsDataSet.Create();
			BenchmarkRunner.Run<VisitedRegionsBenchmark>();
			break;

		case Benchmark.VisitNode_WithQuadrant:
			BenchmarkRunner.Run<VisitNodeWithQuadrantBenchmarks>();
			break;

		case Benchmark.VisitNode_GetQuadrant:
			BenchmarkRunner.Run<QuadDimensionGetQuadrantBenchmarks>();
			break;

		case Benchmark.FixedSizeCache:
			BenchmarkRunner.Run<FixedSizeCacheBenchmarks>();
			break;

		case Benchmark.CoordinateHashing:
			CoordinateHashingTests.ComputeHistograms();
			BenchmarkRunner.Run<CoordinateHashingBenchmarks>();
			break;

		case Benchmark.SquareBoundaryIntersections:
			BenchmarkRunner.Run<SquareBoundaryIntersectionBenchmarks>();
			break;

		case Benchmark.RenderRegion:
			RenderRegions.Run();
			break;
	}
}

enum Benchmark
{
	VisitedRegions,
	VisitNode_WithQuadrant,
	VisitNode_GetQuadrant,
	FixedSizeCache,
	CoordinateHashing,
	SquareBoundaryIntersections,
	RenderRegion,
}
