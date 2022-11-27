namespace Buddhabrot.Core.Boundary;

public static class QuadTreeExtensions
{
    public static int AddChildren(this List<QuadNode> tree, QuadNode ll, QuadNode lr, QuadNode ul, QuadNode ur)
    {
        var index = tree.Count;

        tree.Add(ll);
        tree.Add(lr);
        tree.Add(ul);
        tree.Add(ur);

        return index;
    }

    public static int AddUnknownLeafChildren(this List<QuadNode> tree) =>
        tree.AddChildren(QuadNode.UnknownLeaf, QuadNode.UnknownLeaf, QuadNode.UnknownLeaf, QuadNode.UnknownLeaf);
}