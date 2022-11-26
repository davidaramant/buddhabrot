using Buddhabrot.Core.Boundary.Corners;

namespace Buddhabrot.Core.Boundary;

public static class BoundaryCalculator
{
    public static IReadOnlyList<(RegionId Region, RegionType Type)>
        FindBoundaryAndFilaments(
            BoundaryParameters boundaryParameters,
            Action<string> log,
            CancellationToken cancelToken = default,
            IVisitedRegions? visitedRegionsArg = null)
    {
        var cornerComputer = new RegionCorners(boundaryParameters);
        IVisitedRegions visitedRegions = visitedRegionsArg ?? new ListOfHashSetVisitedRegions(boundaryParameters.Divisions.QuadrantDivisions);
        Queue<RegionId> regionsToCheck = new();
        regionsToCheck.Enqueue(new RegionId(0, 0));

        var returnList = new List<(RegionId, RegionType)>();

        while (regionsToCheck.Count > 0 && !cancelToken.IsCancellationRequested)
        {
            var region = regionsToCheck.Dequeue();

            if (visitedRegions.HasVisited(region))
                continue;

            var corners = cornerComputer.GetRegionCorners(region);

            RegionType regionType;

            if (corners.ContainsBorder)
            {
                regionType = RegionType.Border;
            }
            else if (corners.InsideSet)
            {
                regionType = RegionType.Rejected;
            }
            else
            {
                regionType = cornerComputer.DoesRegionContainFilaments(region) ? RegionType.Filament : RegionType.Rejected;
            }

            visitedRegions.MarkVisited(region, regionType);
            returnList.Add((region, regionType));

            if (regionType != RegionType.Rejected)
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

        log($"Number of visited regions: {visitedRegions.Count:N0}");

        return returnList;

        void AddRegionToCheck(RegionId region)
        {
            if (region.X >= 0 &&
                region.Y >= 0 &&
                region.X < (boundaryParameters.Divisions.QuadrantDivisions * 2) &&
                region.Y < boundaryParameters.Divisions.QuadrantDivisions &&
                !visitedRegions.HasVisited(region))
            {
                regionsToCheck.Enqueue(region);
            }
        }
    }
}