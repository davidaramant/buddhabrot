using Buddhabrot.Core.Boundary.Quadtrees;

namespace Buddhabrot.Core.Boundary;

public sealed class QuadtreeCompressor
{
	private readonly VisitedRegions _visitedRegions;
	private readonly IReadOnlyList<VisitNode> _oldTree;
	private readonly List<RegionNode> _newTree;

#pragma warning disable CS9113 // Parameter is unread.
	private readonly record struct QuadKey(RegionNode SW, RegionNode SE, RegionNode NW, RegionNode NE);
#pragma warning restore CS9113 // Parameter is unread.

	private readonly Dictionary<QuadKey, RegionNode> _cache = new();

	public int Size => _cache.Count;
	public int NumCachedValuesUsed { get; private set; }

	public QuadtreeCompressor(VisitedRegions visitedRegions)
	{
		_visitedRegions = visitedRegions;
		_oldTree = visitedRegions.Nodes;
		var estimatedCapacity = visitedRegions.NodeCount / 2;
		_newTree = new List<RegionNode>(estimatedCapacity);
	}

	public RegionLookup Transform()
	{
		var newNW = Transform(_oldTree[_visitedRegions.Root.GetChildIndex(Quadrant.SW)]);
		var newNE = Transform(_oldTree[_visitedRegions.Root.GetChildIndex(Quadrant.SE)]);

		var rootChildrenIndex = _newTree.AddChildren(RegionNode.Empty, RegionNode.Empty, newNW, newNE);
		// No need to compute the root region type, it will always be border
		_newTree.Add(RegionNode.MakeBranch(LookupRegionType.EmptyToBorder, rootChildrenIndex));

		return new RegionLookup(_newTree, _visitedRegions.Height);
	}

	private RegionNode Transform(VisitNode node)
	{
		if (node.IsLeaf)
			return MakeLeaf(node.RegionType);

		if (node.IsLeafQuad)
			return MakeQuad(MakeLeaf(node.SW), MakeLeaf(node.SE), MakeLeaf(node.NW), MakeLeaf(node.NE));

		return MakeQuad(
			Transform(_oldTree[node.GetChildIndex(Quadrant.SW)]),
			Transform(_oldTree[node.GetChildIndex(Quadrant.SE)]),
			Transform(_oldTree[node.GetChildIndex(Quadrant.NW)]),
			Transform(_oldTree[node.GetChildIndex(Quadrant.NE)])
		);
	}

	public RegionNode MakeQuad(RegionNode sw, RegionNode se, RegionNode nw, RegionNode ne)
	{
		if (sw.IsLeaf && sw == se && se == nw && nw == ne)
		{
			return sw;
		}

		QuadKey key = new(SW: sw, SE: se, NW: nw, NE: ne);
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

	private static RegionNode MakeLeaf(VisitedRegionType type) => RegionNode.MakeLeaf(FilterRegionType(type));

	private static LookupRegionType FilterRegionType(VisitedRegionType type) =>
		type switch
		{
			VisitedRegionType.Border => LookupRegionType.EmptyToBorder,
			VisitedRegionType.Filament => LookupRegionType.EmptyToFilament,
			_ => LookupRegionType.Empty,
		};

	private static LookupRegionType CondenseRegionType(RegionNode sw, RegionNode se, RegionNode nw, RegionNode ne) =>
		CondenseRegionType(sw.RegionType, se.RegionType, nw.RegionType, ne.RegionType);

	public static LookupRegionType CondenseRegionType(
		LookupRegionType sw,
		LookupRegionType se,
		LookupRegionType nw,
		LookupRegionType ne
	)
	{
		int borderCount = 0;
		int filamentCount = 0;

		Count(sw);
		Count(se);
		Count(nw);
		Count(ne);

		if (borderCount == 0 && filamentCount == 0)
			return LookupRegionType.Empty;

		if (borderCount >= filamentCount)
			return LookupRegionType.EmptyToBorder;

		return LookupRegionType.EmptyToFilament;

		void Count(LookupRegionType type)
		{
			switch (type)
			{
				case LookupRegionType.EmptyToBorder:
					borderCount++;
					break;

				case LookupRegionType.EmptyToFilament:
					filamentCount++;
					break;
			}
		}
	}
}
