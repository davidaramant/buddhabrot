using System.Collections.Concurrent;
using System.Numerics;
using Buddhabrot.Core.IterationKernels;

namespace Buddhabrot.Core.Boundary;

public sealed class CornerComputer
{
    private readonly ConcurrentDictionary<CornerPoint, bool> _isPointInSet = new();
    private readonly AreaSizeInfo _areaSizeInfo;

    public CornerComputer(AreaSizeInfo areaSizeInfo) => _areaSizeInfo = areaSizeInfo;

    private async Task<bool> IsPointInSetAsync(CornerPoint point, CancellationToken cancelToken = default)
    {
        if (_isPointInSet.TryGetValue(point, out var inSet))
        {
            return inSet;
        }

        Complex c = new Complex(
            real: point.X * _areaSizeInfo.SideLength - 2,
            imaginary: point.Y * _areaSizeInfo.SideLength);
        inSet = await Task.Run(() => ScalarDoubleKernel.IsInSet(c), cancelToken);
        _isPointInSet.TryAdd(point, inSet);
        return inSet;
    }
    
    public async ValueTask<CornersInSet> GetAreaCornersAsync(AreaId id, CancellationToken cancelToken)
    {
        var lowerLeftTask = IsPointInSetAsync(new CornerPoint(id.X, id.Y), cancelToken);
        var lowerRightTask = IsPointInSetAsync(new CornerPoint(id.X + 1, id.Y), cancelToken);
        var upperRightTask = IsPointInSetAsync(new CornerPoint(id.X + 1, id.Y + 1), cancelToken);
        var upperLeftTask = IsPointInSetAsync(new CornerPoint(id.X, id.Y + 1), cancelToken);

        await Task.WhenAll(lowerLeftTask, lowerRightTask, upperRightTask, upperLeftTask);

        return new CornersInSet(
            lowerLeftTask.Result,
            lowerRightTask.Result,
            upperRightTask.Result,
            upperLeftTask.Result);
    }
}