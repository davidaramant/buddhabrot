using System.Runtime.CompilerServices;

namespace Buddhabrot.Core.Boundary.Quadtrees;

public static class QuadtreeExtensions
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int AddChildren<TNode>(this List<TNode> tree, TNode sw, TNode se, TNode nw, TNode ne)
	{
		var index = tree.Count;

		tree.Add(sw);
		tree.Add(se);
		tree.Add(nw);
		tree.Add(ne);

		return index;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int AddUnknownChildren(this List<VisitNode> tree) =>
		tree.AddChildren(VisitNode.Unknown, VisitNode.Unknown, VisitNode.Unknown, VisitNode.Unknown);
}
