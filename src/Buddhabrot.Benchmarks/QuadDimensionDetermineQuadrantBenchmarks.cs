using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using Buddhabrot.Core.Boundary;

namespace Buddhabrot.Benchmarks;

public class QuadDimensionDetermineQuadrantBenchmarks
{
    private const int Size = 100;
    private readonly RegionId[] _regions = new RegionId[Size];

    [GlobalSetup]
    public void FillArrays()
    {
        var rand = new Random(0);
        for (int i = 0; i < Size; i++)
        {
            _regions[i] = new RegionId(rand.Next(2), rand.Next(2));
        }
    }

    [Benchmark(Baseline = true)]
    public Quadrant Switch()
    {
        var qd = new QuadDimensions(0, 0, 2);

        Quadrant d = default;

        for (int i = 0; i < Size; i++)
        {
            d = DetermineQuadrant(qd, _regions[i]);
        }

        return d;

        static Quadrant DetermineQuadrant(QuadDimensions q, RegionId id)
        {
            var upper = id.Y >= q.Y + q.QuadrantLength;
            var right = id.X >= q.X + q.QuadrantLength;
                                            
            return (upper, right) switch
            {
                (false, false) => Quadrant.LL,
                (false, true) => Quadrant.LR,
                (true, false) => Quadrant.UL,
                (true, true) => Quadrant.UR,
            };
        }
    }

    [Benchmark]
    public Quadrant Branchless()
    {
        var qd = new QuadDimensions(0, 0, 2);

        Quadrant d = default;

        for (int i = 0; i < Size; i++)
        {
            d = DetermineQuadrant(qd, _regions[i]);
        }

        return d;

        static Quadrant DetermineQuadrant(QuadDimensions q, RegionId id)
        {
            var xComponent = (id.X >= q.X + q.QuadrantLength) ? 1 : 0;
            var yComponent = (id.Y >= q.Y + q.QuadrantLength) ? 2 : 0;

            return (Quadrant) (xComponent + yComponent);
        }
    }
    
    [Benchmark]
    public Quadrant BranchlessWithBoolToInt()
    {
        var qd = new QuadDimensions(0, 0, 2);

        Quadrant d = default;

        for (int i = 0; i < Size; i++)
        {
            d = DetermineQuadrant(qd, _regions[i]);
        }

        return d;

        static Quadrant DetermineQuadrant(QuadDimensions q, RegionId id)
        {
            var xComponent = BoolToInt(id.X >= q.X + q.QuadrantLength);
            var yComponent = BoolToInt(id.Y >= q.Y + q.QuadrantLength) << 1;

            return (Quadrant) (xComponent + yComponent);
        }
        
        static unsafe int BoolToInt(bool b) 
        {
            return *(Byte*)&b;         
        }
    }
}