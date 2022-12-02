namespace Buddhabrot.Core.Boundary;

public sealed class VisitedRegionsToRegionLookup
{
    private readonly VisitedRegions _visitedRegions;
    private readonly IReadOnlyList<uint> _oldTree;
    private readonly List<uint> _newTree;
    private readonly Dictionary<(uint LL, uint LR, uint UL, uint UR), uint> _cache = new();

    public int Size => _cache.Count;
    public int NumCachedValuesUsed { get; private set; }

    public VisitedRegionsToRegionLookup(VisitedRegions visitedRegions)
    {
        _visitedRegions = visitedRegions;
        _oldTree = visitedRegions.Nodes;
        _newTree = new List<uint>(_visitedRegions.NodeCount / 2); // TODO: What should this capacity be?
    }

    public RegionLookup Transform()
    {
        var newUL = Normalize(_oldTree[_visitedRegions.Root.GetChildIndex(Quadrant.LL)]);
        var newUR = Normalize(_oldTree[_visitedRegions.Root.GetChildIndex(Quadrant.LR)]);

        var rootChildrenIndex = _newTree.AddChildren(QuadNode2.UnknownLeaf, QuadNode2.UnknownLeaf, newUL, newUR);
        // No need to compute the root region type, it will always be border
        _newTree.Add(QuadNode2.MakeBranch(RegionType.Border, rootChildrenIndex));

        return new RegionLookup(_newTree, _visitedRegions.Height);
    }

    public uint Normalize(uint node) =>
        node.GetNodeType() switch
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

    public static uint NormalizeLeafQuad(uint leafQuad)
    {
        if (leafQuad.GetLL() == leafQuad.GetLR() &&
            leafQuad.GetLR() == leafQuad.GetUL() &&
            leafQuad.GetUL() == leafQuad.GetUR())
        {
            return QuadNode2.MakeLeaf(FilterOutRejected(leafQuad.GetLL()));
        }

        return QuadNode2.MakeLeaf(
            CondenseRegionType(leafQuad.GetLL(), leafQuad.GetLR(), leafQuad.GetUL(), leafQuad.GetUR()),
            FilterOutRejected(leafQuad.GetLL()),
            FilterOutRejected(leafQuad.GetLR()),
            FilterOutRejected(leafQuad.GetUL()),
            FilterOutRejected(leafQuad.GetUR()));
    }

    public uint MakeQuad(uint ll, uint lr, uint ul, uint ur)
    {
        if (ll.GetNodeType() == NodeType.Leaf &&
            ll == lr &&
            lr == ul &&
            ul == ur)
        {
            return ll;
        }

        if (ll.GetNodeType() == NodeType.Leaf &&
            lr.GetNodeType() == NodeType.Leaf &&
            ul.GetNodeType() == NodeType.Leaf &&
            ur.GetNodeType() == NodeType.Leaf)
        {
            return QuadNode2.MakeLeaf(
                CondenseRegionType(ll, lr, ul, ur),
                ll.GetRegionType(),
                lr.GetRegionType(),
                ul.GetRegionType(),
                ur.GetRegionType());
        }

        var key = (ll, lr, ul, ur);
        if (!_cache.TryGetValue(key, out var node))
        {
            var index = _newTree.AddChildren(ll, lr, ul, ur);
            node = QuadNode2.MakeBranch(CondenseRegionType(ll, lr, ul, ur),
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

    public static RegionType CondenseRegionType(uint ll, uint lr, uint ul, uint ur) =>
        CondenseRegionType(ll.GetRegionType(), lr.GetRegionType(), ul.GetRegionType(), ur.GetRegionType());

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