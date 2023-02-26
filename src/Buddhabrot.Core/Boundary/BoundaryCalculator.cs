using Buddhabrot.Core.Boundary.Classifiers;

namespace Buddhabrot.Core.Boundary;

public static class BoundaryCalculator
{
    public static void VisitBoundary(
        IRegionClassifier classifier,
        IVisitedRegions visitedRegions,
        CancellationToken cancelToken = default)
    {
        Queue<RegionId> regionsToCheck = new();
        regionsToCheck.Enqueue(new RegionId(0, 0));

        while (regionsToCheck.Count > 0 && !cancelToken.IsCancellationRequested)
        {
            var region = regionsToCheck.Dequeue();

            if (visitedRegions.HasVisited(region))
                continue;

            var regionType = classifier.ClassifyRegion(region);

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
            // We don't need to check the upper bounds - the set doesn't reach out that far
            if (region is {X: >= 0, Y: >= 0})
            {
                regionsToCheck.Enqueue(region);
            }
        }
    }
}