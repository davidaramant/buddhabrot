namespace Buddhabrot.Core.Boundary;

public sealed class QuadNodeNormalizer
{
    private readonly List<QuadNode> _nodes;
    private readonly Dictionary<(QuadNode LL, QuadNode LR, QuadNode UL, QuadNode UR), QuadNode> _cache = new();
    
    public int Size => _cache.Count;
    public int NumCachedValuesUsed { get; private set; }
    
    public QuadNodeNormalizer(List<QuadNode> nodes) => _nodes = nodes;

    public QuadNode NormalizeLeafQuad(QuadNode leafQuad)
    {
        if (leafQuad.LL == leafQuad.LR &&
            leafQuad.LR == leafQuad.UL &&
            leafQuad.UL == leafQuad.UR)
        {
            return QuadNode.MakeLeaf(leafQuad.LL);
        }

        return leafQuad.WithRegionType(CondenseRegionType(leafQuad.LL, leafQuad.LR, leafQuad.UL, leafQuad.UR));
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
                CondenseRegionType(ll.RegionType, lr.RegionType, ul.RegionType, ur.RegionType),
                ll.RegionType,
                lr.RegionType,
                ul.RegionType,
                ur.RegionType);
        }

        var key = (ll, lr, ul, ur);
        if (!_cache.TryGetValue(key, out var node))
        {
            var index = _nodes.Count;
            node = QuadNode.MakeBranch(CondenseRegionType(ll.RegionType, lr.RegionType, ul.RegionType, ur.RegionType), index);
            _nodes.Add(ll);
            _nodes.Add(lr);
            _nodes.Add(ul);
            _nodes.Add(ur);

            _cache.Add(key, node);
        }
        else
        {
            NumCachedValuesUsed++;
        }

        return node;
    }

    public static RegionType CondenseRegionType(RegionType ll, RegionType lr, RegionType ul, RegionType ur)
    {
        int borderCount = 0;
        int filamentCount = 0;
        int rejectedCount = 0;

        Count(ll);
        Count(lr);
        Count(ul);
        Count(ur);

        if (borderCount == 0 && filamentCount == 0 && rejectedCount == 0)
            return RegionType.Unknown;

        if (borderCount >= filamentCount && borderCount >= rejectedCount)
            return RegionType.Border;

        return filamentCount >= rejectedCount ? RegionType.Filament : RegionType.Rejected;

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

                case RegionType.Rejected:
                    rejectedCount++;
                    break;
            }
        }
    }
}