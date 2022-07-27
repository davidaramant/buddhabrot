namespace Buddhabrot.Core.Boundary;

public static class BoundaryCalculator
{
    public static async Task<IReadOnlyList<AreaId>> FindBoundaryAreasAsync(
        PlotParameters plotParameters,
        IProgress<AreaId>? progress,
        CancellationToken cancelToken = default)
    {
        var cornerComputer = new AreaCorners(plotParameters);
        Dictionary<AreaId, bool> doesAreaContainBorder = new();
        Queue<AreaId> idsToCheck = new();
        idsToCheck.Enqueue(new AreaId(0, 0));

        while (idsToCheck.Any())
        {
            var currentId = idsToCheck.Dequeue();
            
            if (doesAreaContainBorder.ContainsKey(currentId))
                continue;

            var corners = await cornerComputer.GetAreaCornersAsync(currentId, cancelToken);
            
            doesAreaContainBorder.Add(currentId, corners.ContainsBorder);

            if (corners.IsUpperEdge)
            {
                AddIdToCheck(currentId.X, currentId.Y + 1);
            }

            if (corners.IsLowerEdge)
            {
                AddIdToCheck(currentId.X, currentId.Y - 1);
            }

            if (corners.IsLeftEdge)
            {
                AddIdToCheck(currentId.X - 1, currentId.Y);
            }

            if (corners.IsRightEdge)
            {
                AddIdToCheck(currentId.X + 1, currentId.Y);
            }
            
            progress?.Report(currentId);
        }

        return doesAreaContainBorder.Where(pair => pair.Value).Select(pair => pair.Key).ToList();

        void AddIdToCheck(int x, int y)
        {
            if (x < 0 || y < 0 || x >= (plotParameters.VerticalDivisions * 2) || y >= plotParameters.VerticalDivisions)
                return;

            var id = new AreaId(x, y);
            
            if (doesAreaContainBorder.ContainsKey(id))
                return;
            
            idsToCheck.Enqueue(id);
        }
    }
}