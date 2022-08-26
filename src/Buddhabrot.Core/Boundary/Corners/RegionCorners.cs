﻿using System.Collections.Concurrent;
using System.Numerics;
using Buddhabrot.Core.IterationKernels;

namespace Buddhabrot.Core.Boundary.Corners;

public sealed class RegionCorners
{
    private readonly ConcurrentDictionary<CornerId, bool> _isCornerInSet = new();
    private readonly BoundaryParameters _boundaryParams;

    private double RegionWidth => _boundaryParams.Divisions.RegionSideLength;

    public RegionCorners(BoundaryParameters boundaryParams) => _boundaryParams = boundaryParams;

    private async Task<bool> IsCornerInSetAsync(CornerId corner, CancellationToken cancelToken = default)
    {
        if (_isCornerInSet.TryGetValue(corner, out var inSet))
        {
            return inSet;
        }

        Complex c = ToComplex(corner.X, corner.Y);
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

    public bool DoesRegionContainFilaments(RegionId region)
    {
        var center = ToComplex(region.X + 0.5, region.Y + 0.5);

        var distanceToSet = ScalarKernel.FindExteriorDistance(center, _boundaryParams.MaxIterations);
        return distanceToSet <= RegionWidth / 2;
    }

    private Complex ToComplex(double x, double y) => new(real: x * RegionWidth - 2, imaginary: y * RegionWidth);
}