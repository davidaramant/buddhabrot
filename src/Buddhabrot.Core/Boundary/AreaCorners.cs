using System.Collections.Concurrent;
using System.Numerics;
using Buddhabrot.Core.IterationKernels;

namespace Buddhabrot.Core.Boundary;

public sealed class AreaCorners
{
    private readonly ConcurrentDictionary<CornerId, bool> _isCornerInSet = new();
    private readonly BoundaryParameters _boundaryParams;

    public AreaCorners(BoundaryParameters boundaryParams) => _boundaryParams = boundaryParams;

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
            () => ScalarDoubleKernel.FindEscapeTime(c, _boundaryParams.MaxIterations) == EscapeTime.Infinite, 
            cancelToken);
        _isCornerInSet.TryAdd(corner, inSet);
        return inSet;
    }
    
    public async ValueTask<CornersInSet> GetAreaCornersAsync(AreaId id, CancellationToken cancelToken)
    {
        var lowerLeftTask = IsCornerInSetAsync(new CornerId(id.X, id.Y), cancelToken);
        var lowerRightTask = IsCornerInSetAsync(new CornerId(id.X + 1, id.Y), cancelToken);
        var upperRightTask = IsCornerInSetAsync(new CornerId(id.X + 1, id.Y + 1), cancelToken);
        var upperLeftTask = IsCornerInSetAsync(new CornerId(id.X, id.Y + 1), cancelToken);

        await Task.WhenAll(lowerLeftTask, lowerRightTask, upperRightTask, upperLeftTask);

        return new CornersInSet(
            lowerLeftTask.Result,
            lowerRightTask.Result,
            upperRightTask.Result,
            upperLeftTask.Result);
    }
}