namespace Buddhabrot.Core.Boundary;

public sealed record Metrics(
	TimeSpan Duration,
	int NumBorderRegions,
	int NumVisitedRegionNodes,
	int NumRegionLookupNodes
)
{
	public double DeduplicatedSize => (double)NumRegionLookupNodes / NumVisitedRegionNodes;
}
