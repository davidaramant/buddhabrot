using BenchmarkDotNet.Attributes;
using Buddhabrot.Core.Boundary;
using Buddhabrot.Core.Boundary.QuadTrees;

namespace Buddhabrot.Benchmarks;

public class VisitNodeWithQuadrantBenchmarks
{
    private const int Size = 100;
    private readonly Quadrant[] _quadrants = new Quadrant[Size];
    private readonly RegionType[] _types = new RegionType[Size];

    [GlobalSetup]
    public void FillArrays()
    {
        var rand = new Random(0);
        for (int i = 0; i < Size; i++)
        {
            _quadrants[i] = (Quadrant) rand.Next(4);
            _types[i] = (RegionType) rand.Next(4);
        }
    }

    [Benchmark(Baseline = true)]
    public VisitNode Switch()
    {
        var node = VisitNode.Empty;

        for (int i = 0; i < Size; i++)
        {
            node = WithQuadrant(node, _quadrants[i], _types[i]);
        }

        return node;

        static VisitNode WithQuadrant(VisitNode node, Quadrant quadrant, RegionType type) =>
            quadrant switch
            {
                Quadrant.SW => node.WithSW(type),
                Quadrant.SE => node.WithSE(type),
                Quadrant.NW => node.WithNW(type),
                Quadrant.NE => node.WithNE(type),
                _ => throw new Exception("Can't happen")
            };
    }

    [Benchmark]
    public VisitNode Branchless()
    {
        var node = VisitNode.Empty;

        for (int i = 0; i < Size; i++)
        {
            node = WithQuadrant(node, _quadrants[i], _types[i]);
        }

        return node;

        static VisitNode WithQuadrant(VisitNode node, Quadrant quadrant, RegionType type)
        {
            var offset = 10 - 2 * (int) quadrant;
            return new(node.Encoded | ((uint) type << offset) + (int) NodeType.LeafQuad);
        }
    }
}