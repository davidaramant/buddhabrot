namespace Buddhabrot.Core.Boundary.QuadTrees;

public static class QuadTreeExtensions
{
    public static int AddChildren(this List<QuadNode> tree, QuadNode sw, QuadNode se, QuadNode nw, QuadNode ne)
    {
        var index = tree.Count;

        tree.Add(sw);
        tree.Add(se);
        tree.Add(nw);
        tree.Add(ne);

        return index;
    }

    public static int AddUnknownLeafChildren(this List<QuadNode> tree) =>
        tree.AddChildren(QuadNode.UnknownLeaf, QuadNode.UnknownLeaf, QuadNode.UnknownLeaf, QuadNode.UnknownLeaf);
}