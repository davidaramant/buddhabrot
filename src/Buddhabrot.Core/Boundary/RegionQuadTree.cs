namespace Buddhabrot.Core.Boundary;

/// <summary>
/// Tracks which regions have been visited.
/// </summary>
/// <remarks>
/// A mutable quadtree that operates on region coordinates (+Y is UP).
/// It will construct a quad tree that covers [-2,2] on the real axis and [0,4] on the imaginary axis.
/// The region type is NOT computed for branch nodes or leaf quads.
/// </remarks>
public sealed class RegionQuadTree
{
    // Start with a leaf quad - this avoids having to split a leaf with a value into a leaf quad.
    private QuadDimensions _dimensions = new(X: 0, Y: 0, Height: 2);
    private QuadNode _root = QuadNode.UnknownLeafQuad;
    private readonly List<QuadNode> _nodes;

    public int Height => _dimensions.Height;
    public IReadOnlyList<QuadNode> Nodes => _nodes;

    public RegionQuadTree(int capacity = 0) => _nodes = new(capacity);

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
                var childNodeToAdd = dimensions.Height > 3 ? QuadNode.UnknownLeaf : QuadNode.UnknownLeafQuad;

                _nodes.Add(childNodeToAdd);
                _nodes.Add(childNodeToAdd);
                _nodes.Add(childNodeToAdd);
                _nodes.Add(childNodeToAdd);
            }

            nodeIndex = node.GetChildIndex(quadrant);
            node = _nodes[nodeIndex];
            dimensions = dimensions.GetQuadrant(quadrant);
        }

        quadrant = dimensions.DetermineQuadrant(id);
        var updatedNode = node.WithQuadrant(quadrant, type);
        if (nodeIndex != -1)
        {
            _nodes[nodeIndex] = updatedNode;
        }
        else
        {
            _root = updatedNode;
        }
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