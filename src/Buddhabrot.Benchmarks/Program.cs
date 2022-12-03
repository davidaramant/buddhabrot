using System.Diagnostics;
using BenchmarkDotNet.Running;
using Buddhabrot.Core.Boundary;
using Humanizer;

namespace Buddhabrot.Benchmarks;

class Program
{
    static void Main(string[] args)
    {
        //VisitedRegionsBenchmark.CreateDataSet();
        //BenchmarkRunner.Run<VisitedRegionsBenchmark>();

        //BenchmarkRunner.Run<QuadNodeWithQuadrantBenchmarks>();
        //BenchmarkRunner.Run<QuadNodeGetQuadrantBenchmarks>();

        //BenchmarkRunner.Run<QuadDimensionDetermineQuadrantBenchmarks>();
        //BenchmarkRunner.Run<QuadDimensionGetQuadrantBenchmarks>();

        BenchmarkRunner.Run<FixedSizeCacheBenchmarks>();

        // var vrb = new VisitedRegionsBenchmark();
        // vrb.LoadDataSet();
        // Console.Out.WriteLine("Visiting regions...");
        // vrb.UseVisitedRegions(new VisitedRegionsBenchmark.DescribedImplementation(
        //     new VisitedRegions(VisitedRegionsBenchmark.Size.QuadrantDivisions * 2), "Whatever"));
    }

    private static void SimpleBenchmark<T>(Func<T> method, string name)
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
}