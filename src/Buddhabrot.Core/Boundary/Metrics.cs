namespace Buddhabrot.Core.Boundary;

public sealed record Metrics(
	TimeSpan Duration,
	int NumBorderRegions,
	int NumVisitedRegionNodes,
	double PercentVisitedRegionLeafNodes,
	double PercentVisitedRegionLeafQuadNodes,
	double PercentVisitedRegionBranchNodes,
	int NumRegionLookupNodes
)
{
	public double DeduplicatedSize => (double)NumRegionLookupNodes / NumVisitedRegionNodes;
}
