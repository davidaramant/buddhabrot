using System.Numerics;
using Buddhabrot.Core.Calculations;
using Buddhabrot.Core.Utilities;

namespace Buddhabrot.Core.Boundary.Corners;

public sealed class RegionCorners
{
    private readonly FixedSizeCache<CornerId, bool> _isCornerInSet = new(32);
    private readonly BoundaryParameters _boundaryParams;

    public int CacheSize => _isCornerInSet.Count;

    private double RegionWidth => _boundaryParams.Divisions.RegionSideLength;

    public RegionCorners(BoundaryParameters boundaryParams) => _boundaryParams = boundaryParams;

    private bool IsCornerInSet(CornerId corner)
    {
        if (_isCornerInSet.TryGetValue(corner, out var inSet))
        {
            return inSet;
        }

        Complex c = ToComplex(corner.X, corner.Y);
        inSet = ScalarKernel.FindEscapeTime(c, _boundaryParams.MaxIterations) == EscapeTime.Infinite;
        _isCornerInSet.Add(corner, inSet);
        return inSet;
    }

    public CornersInSet GetRegionCorners(RegionId region) =>
        new(
            UpperLeft: IsCornerInSet(region.UpperLeftCorner()),
            UpperRight: IsCornerInSet(region.UpperRightCorner()),
            LowerRight: IsCornerInSet(region.LowerRightCorner()),
            LowerLeft: IsCornerInSet(region.LowerLeftCorner()));

    public bool DoesRegionContainFilaments(RegionId region)
    {
        var center = ToComplex(region.X + 0.5, region.Y + 0.5);

        var distanceToSet = ScalarKernel.FindExteriorDistance(center, _boundaryParams.MaxIterations);
        return distanceToSet <= RegionWidth / 2;
    }

    private Complex ToComplex(double x, double y) => new(real: x * RegionWidth - 2, imaginary: y * RegionWidth);
}