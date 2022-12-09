using Buddhabrot.Core.Boundary.QuadTrees;

namespace Buddhabrot.Core.Boundary;

/// <summary>
/// Tracks which regions have been visited.
/// </summary>
/// <remarks>
/// A mutable quadtree that operates on region coordinates (+Y is UP).
/// It will construct a quad tree that covers [-2,2] on the real axis and [0,4] on the imaginary axis.
/// The region type is NOT computed for branch nodes or leaf quads.
///
/// Initially this is a tree of height 3 leaf nodes at the second level. Having the root be a branch avoids some
/// special cases.
/// </remarks>
public sealed class VisitedRegions : IVisitedRegions
{
    private QuadDimensions _dimensions = new(X: 0, Y: 0, Height: 3);
    private QuadNode _root = QuadNode.MakeLongBranch(0);
    private readonly List<QuadNode> _nodes;

    public int Height => _dimensions.Height;
    public int NodeCount => _nodes.Count + 1;
    public IReadOnlyList<QuadNode> Nodes => _nodes;
    public QuadNode Root => _root;

    public VisitedRegions(int capacity = 0) =>
        _nodes = new(capacity)
        {
            QuadNode.UnknownLeaf,
            QuadNode.UnknownLeaf,
            QuadNode.UnknownLeaf,
            QuadNode.UnknownLeaf
        };

    public void Visit(RegionId id, RegionType type)
    {
        // Do we have to expand the tree?
        if (!_dimensions.Contains(id))
        {
            var index = _nodes.AddChildren(
                _root,
                QuadNode.UnknownLeaf,
                QuadNode.UnknownLeaf,
                QuadNode.UnknownLeaf);

            _dimensions = _dimensions.Expand();
            _root = QuadNode.MakeLongBranch(index);
        }

        var nodeIndex = -1;
        var node = _root;
        var height = _dimensions.Height;
        var halfWidth = _dimensions.QuadrantLength;
        Quadrant quadrant = default;
        var x = id.X;
        var y = id.Y;
        int h = 0;
        int v = 0;

        while (height > 2)
        {
            h = x / halfWidth;
            v = y / halfWidth;

            quadrant = (Quadrant)(h + (v << 1));
            x -= h * halfWidth;
            y -= v * halfWidth;
            halfWidth /= 2;
            height--;

            var nodeType = node.NodeType;
            // The only possible nodes are leaves and branches because of the height check

            if (nodeType == NodeType.Leaf)
            {
                var index = _nodes.AddUnknownLeafChildren();
                _nodes[nodeIndex] = node = QuadNode.MakeLongBranch(index);
            }

            nodeIndex = node.GetLongChildIndex(quadrant);
            node = _nodes[nodeIndex];
        }

        h = x / halfWidth;
        v = y / halfWidth;

        quadrant = (Quadrant)(h + (v << 1));

        var updatedNode = node.WithQuadrant(quadrant, type);
        _nodes[nodeIndex] = updatedNode;
    }

    public bool HasVisited(RegionId id)
    {
        if (!_dimensions.Contains(id))
            return false;

        var node = _root;
        var halfWidth = _dimensions.QuadrantLength;
        var x = id.X;
        var y = id.Y;

        descendTree:

        var h = x / halfWidth;
        var v = y / halfWidth;

        var quadrant = (Quadrant)(h + (v << 1));
        x -= h * halfWidth;
        y -= v * halfWidth;
        halfWidth /= 2;
        
        switch (node.NodeType)
        {
            // We only insert Unknown leaves
            case NodeType.Leaf:
                return false;

            case NodeType.LeafQuad:
                return node.GetQuadrant(quadrant) != RegionType.Unknown;

            case NodeType.LongBranch:
                var index = node.GetLongChildIndex(quadrant);
                node = _nodes[index];
                break;
        }

        goto descendTree;
    }

    public IReadOnlyList<RegionId> GetBoundaryRegions()
    {
        var regions = new List<RegionId>();

        DescendNode(_root, _dimensions, regions);

        return regions;
    }

    private void DescendNode(QuadNode node, QuadDimensions dimensions, List<RegionId> borderRegions)
    {
        // All leaves are going to be empty; skip them
        switch (node.NodeType)
        {
            case NodeType.LeafQuad:
                if (node.LL == RegionType.Border)
                {
                    borderRegions.Add(dimensions.GetRegion(Quadrant.LL));
                }

                if (node.LR == RegionType.Border)
                {
                    borderRegions.Add(dimensions.GetRegion(Quadrant.LR));
                }

                if (node.UL == RegionType.Border)
                {
                    borderRegions.Add(dimensions.GetRegion(Quadrant.UL));
                }

                if (node.UR == RegionType.Border)
                {
                    borderRegions.Add(dimensions.GetRegion(Quadrant.UR));
                }

                break;

            case NodeType.LongBranch:
                DescendNode(_nodes[node.GetLongChildIndex(Quadrant.LL)], dimensions.LL, borderRegions);
                DescendNode(_nodes[node.GetLongChildIndex(Quadrant.LR)], dimensions.LR, borderRegions);
                DescendNode(_nodes[node.GetLongChildIndex(Quadrant.UL)], dimensions.UL, borderRegions);
                DescendNode(_nodes[node.GetLongChildIndex(Quadrant.UR)], dimensions.UR, borderRegions);
                break;
        }
    }
}