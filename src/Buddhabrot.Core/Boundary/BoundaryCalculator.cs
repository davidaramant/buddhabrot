using Buddhabrot.Core.Boundary.Corners;

namespace Buddhabrot.Core.Boundary;

public static class BoundaryCalculator
{
    public static void VisitBoundary(
        BoundaryParameters boundaryParameters,
        IVisitedRegions visitedRegions,
        CancellationToken cancelToken = default)
    {
        var inspector = new RegionInspector(boundaryParameters);
        Queue<RegionId> regionsToCheck = new();
        regionsToCheck.Enqueue(new RegionId(0, 0));

        while (regionsToCheck.Count > 0 && !cancelToken.IsCancellationRequested)
        {
            var region = regionsToCheck.Dequeue();

            if (visitedRegions.HasVisited(region))
                continue;

            var regionType = inspector.ClassifyRegion(region);

            visitedRegions.Visit(region, regionType);

            if (regionType != VisitedRegionType.Rejected)
            {
                AddRegionToCheck(region.MoveUp());
                AddRegionToCheck(region.MoveUpRight());
                AddRegionToCheck(region.MoveRight());
                AddRegionToCheck(region.MoveDownRight());
                AddRegionToCheck(region.MoveDown());
                AddRegionToCheck(region.MoveDownLeft());
                AddRegionToCheck(region.MoveLeft());
                AddRegionToCheck(region.MoveUpLeft());
            }
        }

        void AddRegionToCheck(RegionId region)
        {
            if (region is {X: >= 0, Y: >= 0} &&
                // We don't need to check these - the set doesn't reach out that far
                // region.X < (boundaryParameters.Divisions.QuadrantDivisions * 2) &&
                // region.Y < boundaryParameters.Divisions.QuadrantDivisions &&
                !visitedRegions.HasVisited(region))
            {
                regionsToCheck.Enqueue(region);
            }
        }
    }
}