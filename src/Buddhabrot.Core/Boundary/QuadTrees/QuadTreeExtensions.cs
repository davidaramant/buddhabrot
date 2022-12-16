namespace Buddhabrot.Core.Boundary.QuadTrees;

public static class QuadTreeExtensions
{
    public static int AddChildren<TNode>(this List<TNode> tree, TNode sw, TNode se, TNode nw, TNode ne)
    {
        var index = tree.Count;

        tree.Add(sw);
        tree.Add(se);
        tree.Add(nw);
        tree.Add(ne);

        return index;
    }
    
    public static int AddEmptyChildren(this List<VisitNode> tree) =>
        tree.AddChildren(VisitNode.Empty, VisitNode.Empty, VisitNode.Empty, VisitNode.Empty);
}