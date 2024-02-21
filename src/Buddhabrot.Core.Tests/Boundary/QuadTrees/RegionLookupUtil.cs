using Buddhabrot.Core.Boundary;

namespace Buddhabrot.Core.Tests.Boundary.QuadTrees;

public static class RegionLookupUtil
{
	public static readonly RegionLookup Power2Lookup = Make(
		(0, 0),
		(1, 0),
		(2, 0),
		(2, 1),
		(3, 1),
		(3, 2),
		(4, 2),
		(4, 1),
		(4, 0)
	);

	public static readonly RegionLookup Power3Lookup = Make(
		(0, 0),
		(1, 0),
		(2, 0),
		(3, 0),
		(3, 1),
		(4, 1),
		(4, 0),
		(5, 0),
		(5, 1),
		(5, 2),
		(6, 2),
		(7, 2),
		(8, 2),
		(9, 2),
		(9, 1),
		(9, 0)
	);

	public static RegionLookup Make(params (int X, int Y)[] borderPoints)
	{
		var vr = new VisitedRegions(borderPoints.Length);
		foreach (var p in borderPoints)
		{
			vr.Visit(new RegionId(p.X, p.Y), VisitedRegionType.Border);
		}

		var transformer = new QuadTreeTransformer(vr);
		return transformer.Transform();
	}
}
