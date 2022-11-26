using BenchmarkDotNet.Attributes;
using Buddhabrot.Core.Boundary;

namespace Buddhabrot.Benchmarks;

public class QuadNodeGetQuadrantBenchmarks
{
    private const int Size = 100;
    private readonly Quadrant[] _quadrants = new Quadrant[Size];

    [GlobalSetup]
    public void FillArrays()
    {
        var rand = new Random(0);
        for (int i = 0; i < Size; i++)
        {
            _quadrants[i] = (Quadrant) rand.Next(4);
        }
    }

    [Benchmark(Baseline = true)]
    public RegionType Switch()
    {
        var node = QuadNode.UnknownLeaf;

        RegionType type = default;
        for (int i = 0; i < Size; i++)
        {
            type = GetQuadrant(node, _quadrants[i]);
        }

        return type;

        static RegionType GetQuadrant(QuadNode node, Quadrant quadrant) =>
            quadrant switch
            {
                Quadrant.LL => node.LL,
                Quadrant.LR => node.LR,
                Quadrant.UL => node.UL,
                Quadrant.UR => node.UR,
                _ => throw new Exception("Can't happen")
            };
    }

    [Benchmark]
    public RegionType Branchless()
    {
        var node = QuadNode.UnknownLeaf;

        RegionType type = default;
        for (int i = 0; i < Size; i++)
        {
            type = GetQuadrant(node, _quadrants[i]);
        }

        return type;

        static RegionType GetQuadrant(QuadNode node, Quadrant quadrant)
        {
            var offset = QuadrantOffset(quadrant);
            return (RegionType) (node.Encoded >> offset & 0b11);
        }
    }
    
    private static int QuadrantOffset(Quadrant quadrant) => 10 - 2 * (int) quadrant;
}