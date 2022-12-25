using System.Numerics;
using Buddhabrot.Core.Boundary.QuadTrees;

namespace Buddhabrot.Core.Boundary;

using Type = LookupRegionType;
using Node = RegionNode;
using Cache = Dictionary<(RegionNode SW, RegionNode SE, RegionNode NW, RegionNode NE), RegionNode>;

public static class RegionLookupDiffer
{
    private static readonly IReadOnlyDictionary<(Type Left, Type Right), Type>
        TypeMappings = new Dictionary<(Type Left, Type Right), Type>
        {
            {(Type.Empty, Type.Empty), Type.Empty},
            {(Type.Empty, Type.EmptyToBorder), Type.EmptyToBorder},
            {(Type.Empty, Type.EmptyToFilament), Type.EmptyToFilament},

            {(Type.EmptyToBorder, Type.Empty), Type.BorderToEmpty},
            {(Type.EmptyToBorder, Type.EmptyToBorder), Type.Empty},
            {(Type.EmptyToBorder, Type.EmptyToFilament), Type.BorderToFilament},

            {(Type.EmptyToFilament, Type.Empty), Type.Empty},
            {(Type.EmptyToFilament, Type.EmptyToBorder), Type.FilamentToBorder},
            {(Type.EmptyToFilament, Type.EmptyToFilament), Type.Empty},
        };

    public static RegionLookup Diff(RegionLookup left, RegionLookup right)
    {
        var height = Math.Max(left.Height, right.Height);

        var cache = new Cache();
        var diffTree = new List<Node>();

        var leftRoot = left.Nodes.Last();
        var rightRoot = right.Nodes.Last();

        var nwDiff = Diff(
            left.Nodes, left.Nodes[leftRoot.GetChildIndex(Quadrant.NW)],
            right.Nodes, right.Nodes[rightRoot.GetChildIndex(Quadrant.NW)],
            diffTree, cache);
        var neDiff = Diff(
            left.Nodes, left.Nodes[leftRoot.GetChildIndex(Quadrant.NE)],
            right.Nodes, right.Nodes[rightRoot.GetChildIndex(Quadrant.NE)],
            diffTree, cache);

        var rootChildrenIndex = diffTree.AddChildren(RegionNode.Empty, RegionNode.Empty, nwDiff, neDiff);
        diffTree.Add(
            RegionNode.MakeBranch(CondenseRegionType(RegionNode.Empty, RegionNode.Empty, nwDiff, neDiff),
                rootChildrenIndex));

        return new RegionLookup(diffTree, height);
    }

    private static Node Diff(
        IReadOnlyList<Node> leftTree,
        Node left,
        IReadOnlyList<Node> rightTree,
        Node right,
        List<Node> diffTree,
        Cache cache)
    {
        // TODO: Recurse
        return Node.Empty;
    }

    private static RegionNode MakeQuad(
        List<Node> nodes,
        Cache cache,
        RegionNode sw,
        RegionNode se,
        RegionNode nw,
        RegionNode ne)
    {
        if (sw.IsLeaf &&
            sw == se &&
            se == nw &&
            nw == ne)
        {
            return sw;
        }

        var key = (sw: sw, se: se, nw: nw, ne: ne);
        if (!cache.TryGetValue(key, out var node))
        {
            var index = nodes.AddChildren(sw, se, nw, ne);
            node = RegionNode.MakeBranch(CondenseRegionType(sw, se, nw, ne), index);

            cache.Add(key, node);
        }

        return node;
    }

    public static Type DetermineType(Type left, Type right) => TypeMappings[(left, right)];

    public static Type CondenseRegionType(Node sw, Node se, Node nw, Node ne) =>
        CondenseRegionType(sw.RegionType, se.RegionType, nw.RegionType, ne.RegionType);

    public static Type CondenseRegionType(
        Type sw,
        Type se,
        Type nw,
        Type ne)
    {
        uint groups = 0;
        int emptyToBorderCount = 0;
        int emptyToFilamentCount = 0;
        int borderToEmptyCount = 0;
        int filamentToEmptyCount = 0;

        Count(sw);
        Count(se);
        Count(nw);
        Count(ne);

        var numGroups = BitOperations.PopCount(groups);
        if (numGroups == 0)
            return Type.Empty;

        if (numGroups > 1)
            return Type.MixedDiff;

        if ((groups & 0b1) == 0b1)
            return emptyToBorderCount >= emptyToFilamentCount ? Type.EmptyToBorder : Type.EmptyToFilament;

        if ((groups & 0b10) == 0b10)
            return borderToEmptyCount >= filamentToEmptyCount ? Type.BorderToEmpty : Type.FilamentToEmpty;

        return (groups & 0b100) == 0b100 ? Type.BorderToFilament : Type.FilamentToBorder;

        void Count(Type type)
        {
            switch (type)
            {
                case Type.Empty:
                    break;

                case Type.EmptyToBorder:
                    groups |= 0b1;
                    emptyToBorderCount++;
                    break;

                case Type.EmptyToFilament:
                    groups |= 0b1;
                    emptyToFilamentCount++;
                    break;

                case Type.BorderToEmpty:
                    groups |= 0b10;
                    borderToEmptyCount++;
                    break;

                case Type.FilamentToEmpty:
                    groups |= 0b10;
                    filamentToEmptyCount++;
                    break;

                case Type.BorderToFilament:
                    groups |= 0b100;
                    break;

                case Type.FilamentToBorder:
                    groups |= 0b1000;
                    break;

                case Type.MixedDiff:
                    groups = 0b1111;
                    break;
            }
        }
    }
}