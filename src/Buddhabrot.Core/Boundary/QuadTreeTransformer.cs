using Buddhabrot.Core.Boundary.QuadTrees;

namespace Buddhabrot.Core.Boundary;

public sealed class QuadTreeTransformer
{
    private readonly VisitedRegions _visitedRegions;
    private readonly IReadOnlyList<QuadNode> _oldTree;
    private readonly List<QuadNode> _newTree;
    private readonly Dictionary<(QuadNode SW, QuadNode SE, QuadNode NW, QuadNode NE), QuadNode> _cache = new();

    public int Size => _cache.Count;
    public int NumCachedValuesUsed { get; private set; }

    public QuadTreeTransformer(VisitedRegions visitedRegions)
    {
        _visitedRegions = visitedRegions;
        _oldTree = visitedRegions.Nodes;
        _newTree = new List<QuadNode>(_visitedRegions.NodeCount / 2); // TODO: What should this capacity be?
    }

    public RegionLookup Transform()
    {
        var newUL = Normalize(_oldTree[_visitedRegions.Root.GetLongChildIndex(Quadrant.SW)]);
        var newUR = Normalize(_oldTree[_visitedRegions.Root.GetLongChildIndex(Quadrant.SE)]);

        var rootChildrenIndex = _newTree.AddChildren(QuadNode.UnknownLeaf, QuadNode.UnknownLeaf, newUL, newUR);
        // No need to compute the root region type, it will always be border
        _newTree.Add(QuadNode.MakeBranch(RegionType.Border, rootChildrenIndex));

        return new RegionLookup(_newTree, _visitedRegions.Height);
    }

    public QuadNode Normalize(QuadNode node) =>
        node.NodeType switch
        {
            NodeType.Leaf => node,
            NodeType.LeafQuad => NormalizeLeafQuad(node),
            NodeType.LongBranch => MakeQuad(
                Normalize(_oldTree[node.GetLongChildIndex(Quadrant.SW)]),
                Normalize(_oldTree[node.GetLongChildIndex(Quadrant.SE)]),
                Normalize(_oldTree[node.GetLongChildIndex(Quadrant.NW)]),
                Normalize(_oldTree[node.GetLongChildIndex(Quadrant.NE)])),
            _ => throw new Exception("This can't happen")
        };

    public static QuadNode NormalizeLeafQuad(QuadNode leafQuad)
    {
        if (leafQuad.SW == leafQuad.SE &&
            leafQuad.SE == leafQuad.NW &&
            leafQuad.NW == leafQuad.NE)
        {
            return QuadNode.MakeLeaf(FilterOutRejected(leafQuad.SW));
        }

        return QuadNode.MakeLeaf(
            CondenseRegionType(leafQuad.SW, leafQuad.SE, leafQuad.NW, leafQuad.NE),
            FilterOutRejected(leafQuad.SW),
            FilterOutRejected(leafQuad.SE),
            FilterOutRejected(leafQuad.NW),
            FilterOutRejected(leafQuad.NE));
    }

    public QuadNode MakeQuad(QuadNode sw, QuadNode se, QuadNode nw, QuadNode ne)
    {
        if (sw.NodeType == NodeType.Leaf &&
            sw == se &&
            se == nw &&
            nw == ne)
        {
            return sw;
        }

        if (sw.NodeType == NodeType.Leaf &&
            se.NodeType == NodeType.Leaf &&
            nw.NodeType == NodeType.Leaf &&
            ne.NodeType == NodeType.Leaf)
        {
            return QuadNode.MakeLeaf(
                CondenseRegionType(sw, se, nw, ne),
                sw.RegionType,
                se.RegionType,
                nw.RegionType,
                ne.RegionType);
        }

        var key = (ll: sw, lr: se, ul: nw, ur: ne);
        if (!_cache.TryGetValue(key, out var node))
        {
            var index = _newTree.AddChildren(sw, se, nw, ne);
            node = QuadNode.MakeBranch(CondenseRegionType(sw, se, nw, ne),
                index);

            _cache.Add(key, node);
        }
        else
        {
            NumCachedValuesUsed++;
        }

        return node;
    }

    private static RegionType FilterOutRejected(RegionType type) =>
        type switch
        {
            RegionType.Rejected => RegionType.Empty,
            _ => type
        };

    public static RegionType CondenseRegionType(QuadNode sw, QuadNode se, QuadNode nw, QuadNode ne) =>
        CondenseRegionType(sw.RegionType, se.RegionType, nw.RegionType, ne.RegionType);

    public static RegionType CondenseRegionType(RegionType sw, RegionType se, RegionType nw, RegionType ne)
    {
        int borderCount = 0;
        int filamentCount = 0;

        Count(sw);
        Count(se);
        Count(nw);
        Count(ne);

        if (borderCount == 0 && filamentCount == 0)
            return RegionType.Empty;

        if (borderCount >= filamentCount)
            return RegionType.Border;

        return RegionType.Filament;

        void Count(RegionType type)
        {
            switch (type)
            {
                case RegionType.Border:
                    borderCount++;
                    break;

                case RegionType.Filament:
                    filamentCount++;
                    break;
            }
        }
    }
}