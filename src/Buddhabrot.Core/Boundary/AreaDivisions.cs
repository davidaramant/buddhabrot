namespace Buddhabrot.Core.Boundary;

public readonly record struct AreaDivisions(int VerticalPower)
{
	public int QuadrantDivisions => 1 << VerticalPower;

	public double RegionSideLength => 2.0 / QuadrantDivisions;
	public double RegionArea => RegionSideLength * RegionSideLength;
}
