using Buddhabrot.Core.Boundary.Corners;

namespace Buddhabrot.Core.Boundary;

public static class BoundaryCalculator
{
    public static async Task<IReadOnlyList<(RegionId Region, RegionType Type)>>
        FindBoundaryAndFilamentsAsync(
            BoundaryParameters boundaryParameters,
            CancellationToken cancelToken = default)
    {
        var cornerComputer = new RegionCorners(boundaryParameters);
        HashSet<RegionId> visitedRegions = new();
        Queue<RegionId> regionsToCheck = new();
        regionsToCheck.Enqueue(new RegionId(0, 0));

        var returnList = new List<(RegionId, RegionType)>();

        while (regionsToCheck.Any())
        {
            var region = regionsToCheck.Dequeue();

            if (visitedRegions.Contains(region))
                continue;

            var corners = await cornerComputer.GetRegionCornersAsync(region, cancelToken);

            bool containsFilament = corners.ContainsBorder || cornerComputer.DoesRegionContainFilaments(region);

            var regionType = (corners.ContainsBorder, containsFilament) switch
            {
                (true, _) => RegionType.Border,
                (false, true) => RegionType.Filament,
                _ => RegionType.Empty,
            };

            visitedRegions.Add(region);

            if (regionType != RegionType.Empty)
            {
                returnList.Add((region, regionType));

                AddRegionToCheck(region.MoveUp());
                AddRegionToCheck(region.MoveDown());
                AddRegionToCheck(region.MoveLeft());
                AddRegionToCheck(region.MoveRight());

                AddRegionToCheck(region.MoveUpLeft());
                AddRegionToCheck(region.MoveUpRight());
                AddRegionToCheck(region.MoveDownLeft());
                AddRegionToCheck(region.MoveDownRight());
            }
        }

        return returnList;

        void AddRegionToCheck(RegionId region)
        {
            if (region.X >= 0 &&
                region.Y >= 0 &&
                region.X < (boundaryParameters.VerticalDivisions * 2) &&
                region.Y < boundaryParameters.VerticalDivisions &&
                !visitedRegions.Contains(region))
            {
                regionsToCheck.Enqueue(region);
            }
        }
    }
}