﻿using BenchmarkDotNet.Attributes;
using Buddhabrot.Core.Boundary;

namespace Buddhabrot.Benchmarks;

public class QuadNodeWithQuadrantBenchmarks
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
    public QuadNode Switch()
    {
        var node = QuadNode.UnknownLeaf;

        for (int i = 0; i < Size; i++)
        {
            node = WithQuadrant(node, _quadrants[i], _types[i]);
        }

        return node;

        static QuadNode WithQuadrant(QuadNode node, Quadrant quadrant, RegionType type) =>
            quadrant switch
            {
                Quadrant.LL => node.WithLL(type),
                Quadrant.LR => node.WithLR(type),
                Quadrant.UL => node.WithUL(type),
                Quadrant.UR => node.WithUR(type),
                _ => throw new Exception("Can't happen")
            };
    }

    [Benchmark]
    public QuadNode Branchless()
    {
        var node = QuadNode.UnknownLeaf;

        for (int i = 0; i < Size; i++)
        {
            node = WithQuadrant(node, _quadrants[i], _types[i]);
        }

        return node;

        static QuadNode WithQuadrant(QuadNode node, Quadrant quadrant, RegionType type)
        {
            var offset = 10 - 2 * (int) quadrant;
            return new(node.Encoded | ((uint) type << offset) + (int) NodeType.LeafQuad);
        }
    }
}