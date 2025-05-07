using System.Numerics;
using Buddhabrot.Core.Boundary.Classifiers;

namespace Buddhabrot.Core.Boundary;

public readonly record struct AreaDivisions(int VerticalPower)
{
	public int QuadrantDivisions => 1 << VerticalPower;

	public double RegionSideLength => 2.0 / QuadrantDivisions;
	public double RegionArea => RegionSideLength * RegionSideLength;

	public RegionId LeftStart() => new(0, 0);

	public RegionId RightStart() => new((1 << VerticalPower) + ((1 << VerticalPower) >> 3), 0);

	public Complex ToComplex(CornerId corner) =>
		new(real: corner.X * RegionSideLength - 2, imaginary: corner.Y * RegionSideLength);

	public QuadTreeViewport GetQuadTreeViewport(RegionId region)
	{
		throw new NotImplementedException("Viewport calculation is not implemented yet.");
	}
}
