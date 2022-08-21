using System.Collections.Concurrent;
using System.Numerics;
using Buddhabrot.Core.IterationKernels;

namespace Buddhabrot.Core.Boundary.Corners;

public sealed class RegionCorners
{
    private readonly ConcurrentDictionary<CornerId, bool> _isCornerInSet = new();
    private readonly BoundaryParameters _boundaryParams;

    public RegionCorners(BoundaryParameters boundaryParams) => _boundaryParams = boundaryParams;

    private async Task<bool> IsCornerInSetAsync(CornerId corner, CancellationToken cancelToken = default)
    {
        if (_isCornerInSet.TryGetValue(corner, out var inSet))
        {
            return inSet;
        }

        Complex c = new(
            real: corner.X * _boundaryParams.SideLength - 2,
            imaginary: corner.Y * _boundaryParams.SideLength);
        inSet = await Task.Run(
            () => ScalarKernel.FindEscapeTime(c, _boundaryParams.MaxIterations) == EscapeTime.Infinite,
            cancelToken);
        _isCornerInSet.TryAdd(corner, inSet);
        return inSet;
    }

    public async ValueTask<CornersInSet> GetRegionCornersAsync(RegionId region, CancellationToken cancelToken)
    {
        var upperLeftTask = IsCornerInSetAsync(region.UpperLeftCorner(), cancelToken);
        var upperRightTask = IsCornerInSetAsync(region.UpperRightCorner(), cancelToken);
        var lowerRightTask = IsCornerInSetAsync(region.LowerRightCorner(), cancelToken);
        var lowerLeftTask = IsCornerInSetAsync(region.LowerLeftCorner(), cancelToken);

        await Task.WhenAll(upperLeftTask, upperRightTask, lowerRightTask, lowerLeftTask);

        return new CornersInSet(
            UpperLeft: upperLeftTask.Result,
            UpperRight: upperRightTask.Result,
            LowerRight: lowerRightTask.Result,
            LowerLeft: lowerLeftTask.Result);
    }
}