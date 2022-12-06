using System.Buffers;
using System.Numerics;
using Buddhabrot.Core.Calculations;
using Buddhabrot.Core.Utilities;

namespace Buddhabrot.Core.Boundary.Corners;

public sealed class RegionCorners
{
    private readonly FixedSizeCache<RegionBatchId, BoolVector16> _cachedCorners = new(64,
        defaultKey: RegionBatchId.Invalid,
        getIndex: cbi => cbi.GetHashCode64());
    private readonly FixedSizeCache<RegionBatchId, BoolVector16> _cachedCenters = new(64,
        defaultKey: RegionBatchId.Invalid,
        getIndex: cbi => cbi.GetHashCode64());

    private readonly BoundaryParameters _boundaryParams;

    private double RegionWidth => _boundaryParams.Divisions.RegionSideLength;

    public RegionCorners(BoundaryParameters boundaryParams) => _boundaryParams = boundaryParams;


    private bool IsCornerInSet(CornerId corner)
    {
        var batchId = corner.ToBatchId();
        if (!_cachedCorners.TryGetValue(batchId, out var batch))
        {
            batch = ComputeCornerBatch(batchId);
            _cachedCorners.Add(batchId, batch);
        }

        return batch[corner.GetBatchIndex()];
    }

    public bool DoesRegionContainFilaments(RegionId region)
    {
        var batchId = region.ToBatchId();
        if (!_cachedCenters.TryGetValue(batchId, out var batch))
        {
            batch = ComputeCenterBatch(batchId);
            _cachedCenters.Add(batchId, batch);
        }

        return batch[region.GetBatchIndex()];
    }

    public CornersInSet GetRegionCorners(RegionId region) =>
        new(
            UpperLeft: IsCornerInSet(region.UpperLeftCorner()),
            UpperRight: IsCornerInSet(region.UpperRightCorner()),
            LowerRight: IsCornerInSet(region.LowerRightCorner()),
            LowerLeft: IsCornerInSet(region.LowerLeftCorner()));

    private BoolVector16 ComputeCornerBatch(RegionBatchId id)
    {
        var corners = ArrayPool<Complex>.Shared.Rent(16);
        var inSet = ArrayPool<bool>.Shared.Rent(16);

        var bottomLeftCorner = id.GetBottomLeftCorner();
        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                corners[y * 4 + x] = ToComplex(bottomLeftCorner + new Offset(x, y));
            }
        }

        Parallel.For(0, 16,
            i => { inSet[i] = ScalarKernel.FindEscapeTime(corners[i], _boundaryParams.MaxIterations).IsInfinite; }
        );

        var batch = BoolVector16.Empty;

        for (int i = 0; i < 16; i++)
        {
            if (inSet[i])
            {
                batch = batch.WithBit(i);
            }
        }

        ArrayPool<Complex>.Shared.Return(corners);
        ArrayPool<bool>.Shared.Return(inSet);

        return batch;
    }

    private BoolVector16 ComputeCenterBatch(RegionBatchId id)
    {
        var corners = ArrayPool<Complex>.Shared.Rent(16);
        var centerContainsFilaments = ArrayPool<bool>.Shared.Rent(16);

        var bottomLeftRegion = id.GetBottomLeftRegion();
        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                corners[y * 4 + x] = GetRegionCenter(bottomLeftRegion + new Offset(x, y));
            }
        }

        Parallel.For(0, 16,
            i => { centerContainsFilaments[i] = ScalarKernel.FindExteriorDistance(corners[i], _boundaryParams.MaxIterations) <= RegionWidth / 2; }
        );

        var batch = BoolVector16.Empty;

        for (int i = 0; i < 16; i++)
        {
            if (centerContainsFilaments[i])
            {
                batch = batch.WithBit(i);
            }

        }

        ArrayPool<Complex>.Shared.Return(corners);
        ArrayPool<bool>.Shared.Return(centerContainsFilaments);

        return batch;
    }

    private Complex ToComplex(CornerId id) => ToComplex(id.X, id.Y);
    private Complex GetRegionCenter(RegionId id) => ToComplex(id.X + 0.5, id.Y + 0.5);
    private Complex ToComplex(double x, double y) => new(real: x * RegionWidth - 2, imaginary: y * RegionWidth);
}