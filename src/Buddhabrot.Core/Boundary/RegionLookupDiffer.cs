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

            {(Type.EmptyToFilament, Type.Empty), Type.FilamentToEmpty},
            {(Type.EmptyToFilament, Type.EmptyToBorder), Type.FilamentToBorder},
            {(Type.EmptyToFilament, Type.EmptyToFilament), Type.Empty},
        };

    public static RegionLookup Diff(RegionLookup left, RegionLookup right)
    {
        var height = Math.Max(left.Height, right.Height);

        var cache = new Cache();
        var diffTree = new List<Node>();

        diffTree.Add(Diff(
            left.Nodes, left.Nodes.Last(),
            right.Nodes, right.Nodes.Last(),
            diffTree, cache));

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
        if (left.IsLeaf && right.IsLeaf)
            return Node.MakeLeaf(DetermineType(left.RegionType, right.RegionType));

        if (!left.IsLeaf && !right.IsLeaf)
        {
            var sw = Diff(
                leftTree, leftTree[left.GetChildIndex(Quadrant.SW)],
                rightTree, rightTree[right.GetChildIndex(Quadrant.SW)],
                diffTree, cache);

            var se = Diff(
                leftTree, leftTree[left.GetChildIndex(Quadrant.SE)],
                rightTree, rightTree[right.GetChildIndex(Quadrant.SE)],
                diffTree, cache);

            var nw = Diff(
                leftTree, leftTree[left.GetChildIndex(Quadrant.NW)],
                rightTree, rightTree[right.GetChildIndex(Quadrant.NW)],
                diffTree, cache);

            var ne = Diff(
                leftTree, leftTree[left.GetChildIndex(Quadrant.NE)],
                rightTree, rightTree[right.GetChildIndex(Quadrant.NE)],
                diffTree, cache);

            return MakeQuad(diffTree, cache, sw, se, nw, ne);
        }

        return left.IsLeaf
            ? CopySubtree(rightTree, right, rightType => DetermineType(left.RegionType, rightType), diffTree, cache)
            : CopySubtree(leftTree, left, leftType => DetermineType(leftType, right.RegionType), diffTree, cache);
    }

    private static Node CopySubtree(
        IReadOnlyList<Node> nodes,
        Node node,
        Func<Type, Type> determineType,
        List<Node> diffTree,
        Cache cache)
    {
        if (node.IsLeaf)
            return Node.MakeLeaf(determineType(node.RegionType));

        return MakeQuad(
            diffTree,
            cache,
            sw: CopySubtree(nodes, nodes[node.GetChildIndex(Quadrant.SW)], determineType, diffTree, cache),
            se: CopySubtree(nodes, nodes[node.GetChildIndex(Quadrant.SE)], determineType, diffTree, cache),
            nw: CopySubtree(nodes, nodes[node.GetChildIndex(Quadrant.NW)], determineType, diffTree, cache),
            ne: CopySubtree(nodes, nodes[node.GetChildIndex(Quadrant.NE)], determineType, diffTree, cache));
    }

    private static Node MakeQuad(
        List<Node> diffTree,
        Cache cache,
        Node sw,
        Node se,
        Node nw,
        Node ne)
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
            var index = diffTree.AddChildren(sw, se, nw, ne);
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