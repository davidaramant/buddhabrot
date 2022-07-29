namespace Buddhabrot.Core.Boundary;

public static class BoundaryCalculator
{
    public static async Task<IReadOnlyList<RegionId>> FindBoundaryAsync(
        BoundaryParameters boundaryParameters,
        CancellationToken cancelToken = default)
    {
        var cornerComputer = new RegionCorners(boundaryParameters);
        Dictionary<RegionId, bool> doesRegionContainBorder = new();
        Queue<RegionId> regionsToCheck = new();
        regionsToCheck.Enqueue(new RegionId(0, 0));
        
        while (regionsToCheck.Any())
        {
            var currentRegion = regionsToCheck.Dequeue();

            if (doesRegionContainBorder.ContainsKey(currentRegion))
                continue;

            var corners = await cornerComputer.GetRegionCornersAsync(currentRegion, cancelToken);

            doesRegionContainBorder.Add(currentRegion, corners.ContainsBorder);

            AddRegionToCheckIfTrue(corners.IsUpperEdge, currentRegion.MoveUp);
            AddRegionToCheckIfTrue(corners.IsLowerEdge, currentRegion.MoveDown);
            AddRegionToCheckIfTrue(corners.IsLeftEdge, currentRegion.MoveLeft);
            AddRegionToCheckIfTrue(corners.IsRightEdge, currentRegion.MoveRight);
        }

        return doesRegionContainBorder.Where(pair => pair.Value).Select(pair => pair.Key).ToList();

        void AddRegionToCheckIfTrue(bool value, Func<RegionId> regionGetter)
        {
            if (value)
            {
                AddRegionToCheck(regionGetter());
            }
        }

        void AddRegionToCheck(RegionId region)
        {
            if (region.X >= 0 &&
                region.Y >= 0 &&
                region.X < (boundaryParameters.VerticalDivisions * 2) &&
                region.Y < boundaryParameters.VerticalDivisions &&
                !doesRegionContainBorder.ContainsKey(region))
            {
                regionsToCheck.Enqueue(region);
            }
        }
    }
}