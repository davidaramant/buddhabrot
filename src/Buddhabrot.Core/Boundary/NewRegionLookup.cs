﻿namespace Buddhabrot.Core.Boundary;

public sealed class NewRegionLookup
{
    private readonly IReadOnlyList<QuadNode> _nodes;
    
    public int Height { get; }
    public int Count => _nodes.Count;

    public NewRegionLookup(IReadOnlyList<QuadNode> nodes, int height)
    {
        _nodes = nodes;
        Height = height;
    }


}