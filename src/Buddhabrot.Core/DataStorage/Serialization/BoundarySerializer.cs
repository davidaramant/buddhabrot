using System.Runtime.CompilerServices;
using Buddhabrot.Core.Boundary;
using Buddhabrot.Core.Boundary.Quadtrees;
using Buddhabrot.Core.DataStorage.Serialization.Internal;

namespace Buddhabrot.Core.DataStorage.Serialization;

public static class BoundarySerializer
{
	public static void Save(BoundaryParameters parameters, IEnumerable<RegionId> regions, Stream stream)
	{
		var boundaries = new Boundaries
		{
			VerticalPower = parameters.Divisions.VerticalPower,
			MaximumIterations = parameters.MaxIterations,
			Regions = regions.Select(r => new RegionLocation { X = r.X, Y = r.Y }).ToArray(),
		};
		boundaries.Save(stream);
	}

	public static void Save(RegionLookup lookup, Stream stream)
	{
		var quadTree = new PersistedQuadtree
		{
			Height = lookup.Height,
			Nodes = lookup.Nodes.Select(qn => qn.Encoded).ToArray(),
		};
		quadTree.Save(stream);
	}

	public static (BoundaryParameters Parameters, IReadOnlyList<RegionId> Regions) LoadRegions(Stream stream)
	{
		var boundaries = Boundaries.Load(stream);
		return (
			new BoundaryParameters(new AreaDivisions(boundaries.VerticalPower), boundaries.MaximumIterations),
			boundaries.Regions.Select(rl => new RegionId(X: rl.X, Y: rl.Y)).ToList()
		);
	}

	public static RegionLookup LoadQuadtree(Stream stream)
	{
		var quadTree = PersistedQuadtree.Load(stream);
		return new RegionLookup(Unsafe.As<IReadOnlyList<RegionNode>>(quadTree.Nodes), quadTree.Height);
	}
}
