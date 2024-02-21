using BenchmarkDotNet.Attributes;
using Buddhabrot.Core.Boundary;

namespace Buddhabrot.Benchmarks;

public class CoordinateHashingBenchmarks
{
    public const int NumPoints = 100;
    private RegionId[] _points = Array.Empty<RegionId>();

    [GlobalSetup]
    public void FillArrays()
    {
        var rand = new Random(0);

        _points = new RegionId[NumPoints];
        for (int i = 0; i < NumPoints; i++)
        {
            _points[i] = new RegionId(rand.Next(1000), rand.Next(1000));
        }
    }

    [Benchmark(Baseline = true)]
    public long Normal()
    {
        long sum = 0;
        for (int i = 0; i < NumPoints; i++)
        {
            var x = _points[i].X;
            var y = _points[i].Y;

            var bp64 = (y & 0b111) << 3 | (x & 0b111);

            sum += bp64;
        }

        return sum;
    }

    [Benchmark]
    public long Modulo()
    {
        long sum = 0;
        for (int i = 0; i < NumPoints; i++)
        {
            var x = _points[i].X;
            var y = _points[i].Y;

            var bp64 = ((y << 3) | (x & 0b111)) % 64;

            sum += bp64;
        }

        return sum;
    }
}
