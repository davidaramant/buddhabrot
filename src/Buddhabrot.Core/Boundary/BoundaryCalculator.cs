using Buddhabrot.Core.Boundary.Corners;

namespace Buddhabrot.Core.Boundary;

public static class BoundaryCalculator
{
    public static IReadOnlyList<(RegionId Region, RegionType Type)>
        FindBoundaryAndFilaments(
            BoundaryParameters boundaryParameters,
            CancellationToken cancelToken = default)
    {
        var cornerComputer = new RegionCorners(boundaryParameters);
        HashSet<RegionId> visitedRegions = new();
        Stack<RegionId> regionsToCheck = new();
        regionsToCheck.Push(new RegionId(0, 0));

        var returnList = new List<(RegionId, RegionType)>();

        while (regionsToCheck.Count > 0 && !cancelToken.IsCancellationRequested)
        {
            var region = regionsToCheck.Pop();

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
                region.X < (boundaryParameters.Divisions.QuadrantDivisions * 2) &&
                region.Y < boundaryParameters.Divisions.QuadrantDivisions &&
                !visitedRegions.Contains(region))
            {
                regionsToCheck.Push(region);
            }
        }
    }
}