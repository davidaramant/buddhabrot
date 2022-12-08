using Buddhabrot.Core.Boundary.QuadTrees;

namespace Buddhabrot.Core.Boundary;

public sealed class QuadTreeTransformer
{
    private readonly VisitedRegions _visitedRegions;
    private readonly IReadOnlyList<QuadNode> _oldTree;
    private readonly List<QuadNode> _newTree;
    private readonly Dictionary<(QuadNode LL, QuadNode LR, QuadNode UL, QuadNode UR), QuadNode> _cache = new();

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
        var newUL = Normalize(_oldTree[_visitedRegions.Root.GetChildIndex(Quadrant.LL)]);
        var newUR = Normalize(_oldTree[_visitedRegions.Root.GetChildIndex(Quadrant.LR)]);

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
            NodeType.Branch => MakeQuad(
                Normalize(_oldTree[node.GetChildIndex(Quadrant.LL)]),
                Normalize(_oldTree[node.GetChildIndex(Quadrant.LR)]),
                Normalize(_oldTree[node.GetChildIndex(Quadrant.UL)]),
                Normalize(_oldTree[node.GetChildIndex(Quadrant.UR)])),
            _ => throw new Exception("This can't happen")
        };

    public static QuadNode NormalizeLeafQuad(QuadNode leafQuad)
    {
        if (leafQuad.LL == leafQuad.LR &&
            leafQuad.LR == leafQuad.UL &&
            leafQuad.UL == leafQuad.UR)
        {
            return QuadNode.MakeLeaf(FilterOutRejected(leafQuad.LL));
        }

        return QuadNode.MakeLeaf(
            CondenseRegionType(leafQuad.LL, leafQuad.LR, leafQuad.UL, leafQuad.UR),
            FilterOutRejected(leafQuad.LL),
            FilterOutRejected(leafQuad.LR),
            FilterOutRejected(leafQuad.UL),
            FilterOutRejected(leafQuad.UR));
    }

    public QuadNode MakeQuad(QuadNode ll, QuadNode lr, QuadNode ul, QuadNode ur)
    {
        if (ll.NodeType == NodeType.Leaf &&
            ll == lr &&
            lr == ul &&
            ul == ur)
        {
            return ll;
        }

        if (ll.NodeType == NodeType.Leaf &&
            lr.NodeType == NodeType.Leaf &&
            ul.NodeType == NodeType.Leaf &&
            ur.NodeType == NodeType.Leaf)
        {
            return QuadNode.MakeLeaf(
                CondenseRegionType(ll, lr, ul, ur),
                ll.RegionType,
                lr.RegionType,
                ul.RegionType,
                ur.RegionType);
        }

        var key = (ll, lr, ul, ur);
        if (!_cache.TryGetValue(key, out var node))
        {
            var index = _newTree.AddChildren(ll, lr, ul, ur);
            node = QuadNode.MakeBranch(CondenseRegionType(ll, lr, ul, ur),
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
            RegionType.Rejected => RegionType.Unknown,
            _ => type
        };

    public static RegionType CondenseRegionType(QuadNode ll, QuadNode lr, QuadNode ul, QuadNode ur) =>
        CondenseRegionType(ll.RegionType, lr.RegionType, ul.RegionType, ur.RegionType);

    public static RegionType CondenseRegionType(RegionType ll, RegionType lr, RegionType ul, RegionType ur)
    {
        int borderCount = 0;
        int filamentCount = 0;

        Count(ll);
        Count(lr);
        Count(ul);
        Count(ur);

        if (borderCount == 0 && filamentCount == 0)
            return RegionType.Unknown;

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