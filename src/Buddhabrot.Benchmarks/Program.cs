using BenchmarkDotNet.Running;

namespace Buddhabrot.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<AutomaticIterationLimitBenchmarks>();
        }
    }
}
