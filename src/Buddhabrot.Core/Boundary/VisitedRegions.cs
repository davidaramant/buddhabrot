namespace Buddhabrot.Core.Boundary;

/// <summary>
/// Tracks which regions have been visited.
/// </summary>
/// <remarks>
/// A mutable quadtree that operates on region coordinates (+Y is UP).
/// It will construct a quad tree that covers [-2,2] on the real axis and [0,4] on the imaginary axis.
/// The region type is NOT computed for branch nodes or leaf quads.
///
/// Initially this is a tree of height 3 leaf nodes at the second level. Having a the root be a branch avoids some
/// special cases.
/// </remarks>
public sealed class VisitedRegions : IVisitedRegions
{
    private QuadDimensions _dimensions = new(X: 0, Y: 0, Height: 3);
    private QuadNode _root = QuadNode.MakeBranch(RegionType.Unknown, 0);
    private readonly List<QuadNode> _nodes;

    public int Height => _dimensions.Height;
    public int NodeCount => _nodes.Count + 1;
    public IReadOnlyList<QuadNode> Nodes => _nodes;

    public VisitedRegions(int capacity = 0) =>
        _nodes = new(capacity)
        {
            QuadNode.UnknownLeaf,
            QuadNode.UnknownLeaf,
            QuadNode.UnknownLeaf,
            QuadNode.UnknownLeaf
        };

    public void MarkVisited(RegionId id, RegionType type)
    {
        // Do we have to expand the tree?
        if (!_dimensions.Contains(id))
        {
            var index = _nodes.Count;
            _nodes.Add(_root);
            _nodes.Add(QuadNode.UnknownLeaf);
            _nodes.Add(QuadNode.UnknownLeaf);
            _nodes.Add(QuadNode.UnknownLeaf);

            _dimensions = _dimensions.Expand();
            _root = QuadNode.MakeBranch(RegionType.Unknown, index);
        }

        var nodeIndex = -1;
        var node = _root;
        var dimensions = _dimensions;
        Quadrant quadrant = default;

        while (dimensions.Height > 2)
        {
            quadrant = dimensions.DetermineQuadrant(id);

            var nodeType = node.NodeType;
            // The only possible nodes are leaves and branches because of the height check

            if (nodeType == NodeType.Leaf)
            {
                var index = _nodes.Count;
                _nodes[nodeIndex] = node = QuadNode.MakeBranch(RegionType.Unknown, index);

                _nodes.Add(QuadNode.UnknownLeaf);
                _nodes.Add(QuadNode.UnknownLeaf);
                _nodes.Add(QuadNode.UnknownLeaf);
                _nodes.Add(QuadNode.UnknownLeaf);
            }

            nodeIndex = node.GetChildIndex(quadrant);
            node = _nodes[nodeIndex];
            dimensions = dimensions.GetQuadrant(quadrant);
        }

        quadrant = dimensions.DetermineQuadrant(id);
        var updatedNode = node.WithQuadrant(quadrant, type);
        _nodes[nodeIndex] = updatedNode;
    }

    public bool HasVisited(RegionId id)
    {
        if (!_dimensions.Contains(id))
            return false;

        var node = _root;
        var dimensions = _dimensions;

        descendTree:

        var quadrant = dimensions.DetermineQuadrant(id);
        switch (node.NodeType)
        {
            // We only insert Unknown leaves
            case NodeType.Leaf:
                return false;

            case NodeType.LeafQuad:
                return node.GetChildRegionType(quadrant) != RegionType.Unknown;

            case NodeType.Branch:
                var index = node.GetChildIndex(quadrant);
                node = _nodes[index];
                dimensions = dimensions.GetQuadrant(quadrant);
                break;
        }

        goto descendTree;
    }

    public RegionLookup TransformToRegionLookup()
    {
        throw new NotImplementedException();
    }

    public IReadOnlyList<RegionId> GetBoundaryRegions()
    {
        throw new NotImplementedException();
    }
}