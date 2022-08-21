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
        Dictionary<RegionId, RegionType> regionLookup = new();
        Queue<RegionId> regionsToCheck = new();
        regionsToCheck.Enqueue(new RegionId(0, 0));

        while (regionsToCheck.Any())
        {
            var currentRegion = regionsToCheck.Dequeue();

            if (regionLookup.ContainsKey(currentRegion))
                continue;

            var corners = await cornerComputer.GetRegionCornersAsync(currentRegion, cancelToken);

            bool containsFilament = corners.ContainsBorder || cornerComputer.DoesRegionContainFilaments(currentRegion);

            var regionType = (corners.ContainsBorder, containsFilament) switch
            {
                (true, _) => RegionType.Border,
                (false, true) => RegionType.Filament,
                _ => RegionType.Empty,
            };

            regionLookup.Add(currentRegion, regionType);

            if (regionType != RegionType.Empty)
            {
                AddRegionToCheck(currentRegion.MoveUp());
                AddRegionToCheck(currentRegion.MoveDown());
                AddRegionToCheck(currentRegion.MoveLeft());
                AddRegionToCheck(currentRegion.MoveRight());
            }
        }

        return regionLookup
            .Where(pair => pair.Value != RegionType.Empty)
            .Select(pair => (pair.Key, pair.Value))
            .ToList();

        void AddRegionToCheck(RegionId region)
        {
            if (region.X >= 0 &&
                region.Y >= 0 &&
                region.X < (boundaryParameters.VerticalDivisions * 2) &&
                region.Y < boundaryParameters.VerticalDivisions &&
                !regionLookup.ContainsKey(region))
            {
                regionsToCheck.Enqueue(region);
            }
        }
    }
}