using Buddhabrot.Core.Boundary.Corners;

namespace Buddhabrot.Core.Boundary;

public static class BoundaryCalculator
{
    public static IReadOnlyList<(RegionId Region, RegionType Type)>
        FindBoundaryAndFilaments(
            BoundaryParameters boundaryParameters,
            Action<string> log,
            CancellationToken cancelToken = default,
            Action<RegionId, RegionType>? logVisitedArea = null,
            IVisitedRegions? visitedRegionsArg = null)
    {
        var logVisit = logVisitedArea ?? new Action<RegionId, RegionType>((_, _) => { });
        var cornerComputer = new RegionCorners(boundaryParameters);
        IVisitedRegions visitedRegions = visitedRegionsArg ?? new VisitedRegions();
        Queue<RegionId> regionsToCheck = new();
        regionsToCheck.Enqueue(new RegionId(0, 0));

        var returnList = new List<(RegionId, RegionType)>();

        while (regionsToCheck.Count > 0 && !cancelToken.IsCancellationRequested)
        {
            var region = regionsToCheck.Dequeue();

            if (visitedRegions.Contains(region))
                continue;

            var corners = cornerComputer.GetRegionCorners(region);

            RegionType regionType;

            if (corners.ContainsBorder)
            {
                regionType = RegionType.Border;
            }
            else if (corners.InsideSet)
            {
                regionType = RegionType.InSet;
            }
            else
            {
                regionType = cornerComputer.DoesRegionContainFilaments(region) ? RegionType.Filament : RegionType.Empty;
            }

            visitedRegions.Add(region);

            logVisit(region, regionType);

            if (regionType != RegionType.Empty)
            {
                returnList.Add((region, regionType));

                if (regionType != RegionType.InSet)
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
        }

        log($"Number of visited regions: {visitedRegions.Count:N0}");

        return returnList;

        void AddRegionToCheck(RegionId region)
        {
            if (region.X >= 0 &&
                region.Y >= 0 &&
                region.X < (boundaryParameters.Divisions.QuadrantDivisions * 2) &&
                region.Y < boundaryParameters.Divisions.QuadrantDivisions &&
                !visitedRegions.Contains(region))
            {
                regionsToCheck.Enqueue(region);
            }
        }
    }
}