using Buddhabrot.Core.Boundary.QuadTrees;

namespace Buddhabrot.Core.Boundary;

/// <summary>
/// Tracks which regions have been visited.
/// </summary>
/// <remarks>
/// A mutable quadtree that operates on region coordinates (+Y is UP).
/// It will construct a quad tree that covers [-2,2] on the real axis and [0,4] on the imaginary axis.
///
/// Initially this is a tree of height 3 leaf nodes at the second level. Having the root be a branch avoids some
/// special cases.
/// </remarks>
public sealed class VisitedRegions : IVisitedRegions
{
    private QuadDimensions _dimensions = new(X: 0, Y: 0, Height: 3);
    private VisitNode _root = VisitNode.MakeBranch(0);
    private readonly List<VisitNode> _nodes;

    public int Height => _dimensions.Height;
    public int NodeCount => _nodes.Count + 1;
    public IReadOnlyList<VisitNode> Nodes => _nodes;
    public VisitNode Root => _root;

    public VisitedRegions(int capacity = 0) =>
        _nodes = new(capacity)
        {
            VisitNode.Unknown,
            VisitNode.Unknown,
            VisitNode.Unknown,
            VisitNode.Unknown
        };

    public void Visit(RegionId id, VisitedRegionType type)
    {
        // Do we have to expand the tree?
        if (!_dimensions.Contains(id))
        {
            var index = _nodes.AddChildren(
                _root,
                VisitNode.Unknown,
                VisitNode.Unknown,
                VisitNode.Unknown);

            _dimensions = _dimensions.Expand();
            _root = VisitNode.MakeBranch(index);
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

            // The only possible nodes are leaves and branches because of the height check
            if (node.IsLeaf)
            {
                var index = _nodes.AddUnknownChildren();
                _nodes[nodeIndex] = node = VisitNode.MakeBranch(index);
            }

            nodeIndex = node.GetChildIndex(quadrant);
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

        // We only insert Unknown leaves
        if (node.IsLeaf)
            return false;

        if(node.IsLeafQuad)
            return node.GetQuadrant(quadrant) != VisitedRegionType.Unknown;
        
        // Branch
        var index = node.GetChildIndex(quadrant);
        node = _nodes[index];
        
        goto descendTree;
    }

    public IReadOnlyList<RegionId> GetBoundaryRegions()
    {
        var regions = new List<RegionId>();

        DescendNode(_root, _dimensions, regions);

        return regions;
    }

    private void DescendNode(VisitNode node, QuadDimensions dimensions, List<RegionId> borderRegions)
    {
        // All leaves are going to be empty; skip them
        if (node.IsLeafQuad)
        {
            if (node.SW == VisitedRegionType.Border)
            {
                borderRegions.Add(dimensions.GetRegion(Quadrant.SW));
            }

            if (node.SE == VisitedRegionType.Border)
            {
                borderRegions.Add(dimensions.GetRegion(Quadrant.SE));
            }

            if (node.NW == VisitedRegionType.Border)
            {
                borderRegions.Add(dimensions.GetRegion(Quadrant.NW));
            }

            if (node.NE == VisitedRegionType.Border)
            {
                borderRegions.Add(dimensions.GetRegion(Quadrant.NE));
            }
        }
        else if (node.IsBranch)
        {
            DescendNode(_nodes[node.GetChildIndex(Quadrant.SW)], dimensions.SW, borderRegions);
            DescendNode(_nodes[node.GetChildIndex(Quadrant.SE)], dimensions.SE, borderRegions);
            DescendNode(_nodes[node.GetChildIndex(Quadrant.NW)], dimensions.NW, borderRegions);
            DescendNode(_nodes[node.GetChildIndex(Quadrant.NE)], dimensions.NE, borderRegions);
        }
    }
}