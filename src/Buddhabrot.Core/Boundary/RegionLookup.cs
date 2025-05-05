using System.Drawing;
using Buddhabrot.Core.Boundary.QuadTrees;
using Buddhabrot.Core.ExtensionMethods.Drawing;
using SkiaSharp;

namespace Buddhabrot.Core.Boundary;

public sealed class RegionLookup
{
	// Instance is reused to give the poor GC a rest
	private readonly Stack<(SquareBoundary, RegionNode)> _toCheck = new();
	public IReadOnlyList<RegionNode> Nodes { get; }
	private readonly RegionNode _root;

	public int Height { get; }
	public int NodeCount => Nodes.Count;

	public static readonly RegionLookup Empty = new();

	private RegionLookup()
	{
		Nodes = [RegionNode.Empty];
		Height = 1;
		_root = Nodes[^1];
	}

	public RegionLookup(IReadOnlyList<RegionNode> nodes, int height)
	{
		Nodes = nodes;
		Height = height;
		_root = nodes[^1];
	}

	// TODO: Remove when rendering has been moved to Core
	public void GetVisibleAreas(
		SquareBoundary bounds,
		IEnumerable<Rectangle> searchAreas,
		ICollection<RegionArea> visibleAreas
	) =>
		GetVisibleAreas(
			bounds,
			searchAreas.Select(r => new SKRectI(left: r.Left, top: r.Top, right: r.Right, bottom: r.Bottom)),
			visibleAreas
		);

	public void GetVisibleAreas(
		SquareBoundary bounds,
		IEnumerable<SKRectI> searchAreas,
		ICollection<RegionArea> visibleAreas
	)
	{
		foreach (var searchArea in searchAreas)
		{
			_toCheck.Push((bounds, _root));

			while (_toCheck.Any())
			{
				var (boundary, currentQuad) = _toCheck.Pop();

				if (currentQuad.RegionType == LookupRegionType.Empty)
					continue;

				var intersection = boundary.IntersectWith(searchArea);
				if (intersection.IsInvalid())
					continue;

				if (currentQuad.IsLeaf || boundary.IsPoint)
				{
					visibleAreas.Add(new(intersection, currentQuad.RegionType));
				}
				else
				{
					_toCheck.Push((boundary.SW, Nodes[currentQuad.GetChildIndex(Quadrant.SW)]));
					_toCheck.Push((boundary.SE, Nodes[currentQuad.GetChildIndex(Quadrant.SE)]));
					_toCheck.Push((boundary.NW, Nodes[currentQuad.GetChildIndex(Quadrant.NW)]));
					_toCheck.Push((boundary.NE, Nodes[currentQuad.GetChildIndex(Quadrant.NE)]));
				}
			}

			// Check the mirrored values to build the bottom of the set
			_toCheck.Push((bounds, _root));

			while (_toCheck.Any())
			{
				var (boundary, currentQuad) = _toCheck.Pop();

				if (currentQuad.RegionType == LookupRegionType.Empty)
					continue;

				var intersection = boundary.IntersectWith(searchArea);
				if (intersection.IsInvalid())
					continue;

				if (currentQuad.IsLeaf || boundary.IsPoint)
				{
					visibleAreas.Add(new(intersection, currentQuad.RegionType));
				}
				else
				{
					_toCheck.Push((boundary.NW, Nodes[currentQuad.GetChildIndex(Quadrant.SW)]));
					_toCheck.Push((boundary.NE, Nodes[currentQuad.GetChildIndex(Quadrant.SE)]));
					_toCheck.Push((boundary.SW, Nodes[currentQuad.GetChildIndex(Quadrant.NW)]));
					_toCheck.Push((boundary.SE, Nodes[currentQuad.GetChildIndex(Quadrant.NE)]));
				}
			}
		}
	}
}
