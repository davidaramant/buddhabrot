using Buddhabrot.Core.Boundary.QuadTrees;

namespace Buddhabrot.Core.Boundary;

public sealed class QuadTreeTransformer
{
    private readonly VisitedRegions _visitedRegions;
    private readonly IReadOnlyList<VisitNode> _oldTree;
    private readonly List<RegionNode> _newTree;

    private readonly Dictionary<(RegionNode SW, RegionNode SE, RegionNode NW, RegionNode NE), RegionNode>
        _cache = new();

    public int Size => _cache.Count;
    public int NumCachedValuesUsed { get; private set; }

    public QuadTreeTransformer(VisitedRegions visitedRegions)
    {
        _visitedRegions = visitedRegions;
        _oldTree = visitedRegions.Nodes;
        _newTree = new List<RegionNode>(_visitedRegions.NodeCount / 2); // TODO: What should this capacity be?
    }

    public RegionLookup Transform()
    {
        var newNW = Normalize(_oldTree[_visitedRegions.Root.GetChildIndex(Quadrant.SW)]);
        var newNE = Normalize(_oldTree[_visitedRegions.Root.GetChildIndex(Quadrant.SE)]);

        var rootChildrenIndex = _newTree.AddChildren(RegionNode.Empty, RegionNode.Empty, newNW, newNE);
        // No need to compute the root region type, it will always be border
        _newTree.Add(RegionNode.MakeBranch(RegionType.Border, rootChildrenIndex));

        return new RegionLookup(_newTree, _visitedRegions.Height);
    }

    public RegionNode Normalize(VisitNode node)
    {
        if (node.IsLeaf)
            return MakeLeaf(node.RegionType);

        if (node.IsLeafQuad)
            return MakeQuad(
                MakeLeaf(node.SW),
                MakeLeaf(node.SE),
                MakeLeaf(node.NW),
                MakeLeaf(node.NE));

        return MakeQuad(
            Normalize(_oldTree[node.GetChildIndex(Quadrant.SW)]),
            Normalize(_oldTree[node.GetChildIndex(Quadrant.SE)]),
            Normalize(_oldTree[node.GetChildIndex(Quadrant.NW)]),
            Normalize(_oldTree[node.GetChildIndex(Quadrant.NE)]));
    }

    public RegionNode MakeQuad(RegionNode sw, RegionNode se, RegionNode nw, RegionNode ne)
    {
        if (sw.IsLeaf &&
            sw == se &&
            se == nw &&
            nw == ne)
        {
            return sw;
        }

        var key = (sw: sw, se: se, nw: nw, ne: ne);
        if (!_cache.TryGetValue(key, out var node))
        {
            var index = _newTree.AddChildren(sw, se, nw, ne);
            node = RegionNode.MakeBranch(CondenseRegionType(sw, se, nw, ne), index);

            _cache.Add(key, node);
        }
        else
        {
            NumCachedValuesUsed++;
        }

        return node;
    }

    private static RegionNode MakeLeaf(RegionType type) => RegionNode.MakeLeaf(FilterRegionType(type));

    private static RegionType FilterRegionType(RegionType type) =>
        type switch
        {
            RegionType.InSet => RegionType.Empty,
            RegionType.Rejected => RegionType.Empty,
            _ => type
        };

    public static RegionType CondenseRegionType(RegionNode sw, RegionNode se, RegionNode nw, RegionNode ne) =>
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