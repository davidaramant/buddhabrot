namespace Buddhabrot.Core.Boundary;

public static class QuadTreeExtensions
{
    public static int AddChildren(this List<uint> tree, uint ll, uint lr, uint ul, uint ur)
    {
        var index = tree.Count;

        tree.Add(ll);
        tree.Add(lr);
        tree.Add(ul);
        tree.Add(ur);

        return index;
    }

    public static int AddUnknownLeafChildren(this List<uint> tree) =>
        tree.AddChildren(QuadNode2.UnknownLeaf, QuadNode2.UnknownLeaf, QuadNode2.UnknownLeaf, QuadNode2.UnknownLeaf);
}