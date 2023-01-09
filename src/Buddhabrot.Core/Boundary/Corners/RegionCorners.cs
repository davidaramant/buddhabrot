using System.Buffers;
using System.Collections.Specialized;
using System.Numerics;
using Buddhabrot.Core.Calculations;
using Buddhabrot.Core.Utilities;

namespace Buddhabrot.Core.Boundary.Corners;

public sealed class RegionCorners
{
    private readonly FixedSizeCache<RegionBatchId, BoolVector16> _cachedCorners = new(64,
        defaultKey: RegionBatchId.Invalid,
        getIndex: cbi => cbi.GetHashCode64());

    private readonly FixedSizeCache<RegionBatchId, BoolVector8> _cachedCenters = new(64,
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

    public VisitedRegionType CheckRegionForFilaments(RegionId region)
    {
        var batchId = region.ToBatchId();
        if (!_cachedCenters.TryGetValue(batchId, out var batch))
        {
            batch = ComputeCenterBatch(batchId);
            _cachedCenters.Add(batchId, batch);
        }

        var index = region.GetBatchIndex() * 2;
        if (!batch[index])
            return VisitedRegionType.Rejected;

        return batch[index + 1] ? VisitedRegionType.Border : VisitedRegionType.Filament;
    }

    public RegionClassification ClassifyRegion(RegionId region)
    {
        int numCorners = 0;

        void CheckCorner(CornerId corner)
        {
            if (IsCornerInSet(corner))
            {
                numCorners++;
            }
        }
        CheckCorner(region.LowerLeftCorner());
        CheckCorner(region.LowerRightCorner());
        CheckCorner(region.UpperLeftCorner());
        CheckCorner(region.UpperRightCorner());

        return numCorners switch
        {
            0 => RegionClassification.NoCornersInSet,
            4 => RegionClassification.AllCornersInSet,
            _ => RegionClassification.MixedCorners,
        };
    }

    private BoolVector16 ComputeCornerBatch(RegionBatchId id)
    {
        var corners = ArrayPool<Complex>.Shared.Rent(RegionBatchId.CornerArea);
        var inSet = ArrayPool<bool>.Shared.Rent(RegionBatchId.CornerArea);

        var bottomLeftCorner = id.GetBottomLeftCorner();
        for (int y = 0; y < RegionBatchId.CornerWidth; y++)
        {
            for (int x = 0; x < RegionBatchId.CornerWidth; x++)
            {
                corners[y * RegionBatchId.CornerWidth + x] = ToComplex(bottomLeftCorner + new Offset(x, y));
            }
        }

        Parallel.For(0, RegionBatchId.CornerArea,
            i => { inSet[i] = ScalarKernel.FindEscapeTime(corners[i], _boundaryParams.MaxIterations).IsInfinite; }
        );

        var batch = BoolVector16.Empty;

        for (int i = 0; i < RegionBatchId.CornerArea; i++)
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

    private BoolVector8 ComputeCenterBatch(RegionBatchId id)
    {
        var centers = ArrayPool<Complex>.Shared.Rent(RegionBatchId.RegionArea);
        var centerContainsFilaments = ArrayPool<VisitedRegionType>.Shared.Rent(RegionBatchId.RegionArea);

        var bottomLeftRegion = id.GetBottomLeftRegion();
        for (int y = 0; y < RegionBatchId.RegionWidth; y++)
        {
            for (int x = 0; x < RegionBatchId.RegionWidth; x++)
            {
                centers[y * RegionBatchId.RegionWidth + x] = GetRegionCenter(bottomLeftRegion + new Offset(x, y));
            }
        }

        Parallel.For(0, 4,
            i =>
            {
                var distance = ScalarKernel.FindExteriorDistance(centers[i], _boundaryParams.MaxIterations);

                centerContainsFilaments[i] = distance switch
                {
                    double.MaxValue => VisitedRegionType.Border,
                    var d when d <= (RegionWidth / 2) => VisitedRegionType.Filament,
                    _ => VisitedRegionType.Rejected
                };
            }
        );

        var batch = BoolVector8.Empty;

        for (int i = 0; i < RegionBatchId.RegionArea; i++)
        {
            var type = centerContainsFilaments[i];

            var index = i * 2;

            switch (type)
            {
                case VisitedRegionType.Border:
                    batch = batch.WithBit(index).WithBit(index + 1);
                    break;
                case VisitedRegionType.Filament:
                    batch = batch.WithBit(index);
                    break;
            }
        }

        ArrayPool<Complex>.Shared.Return(centers);
        ArrayPool<VisitedRegionType>.Shared.Return(centerContainsFilaments);

        return batch;
    }

    private Complex ToComplex(CornerId id) => ToComplex(id.X, id.Y);
    private Complex GetRegionCenter(RegionId id) => ToComplex(id.X + 0.5, id.Y + 0.5);
    private Complex ToComplex(double x, double y) => new(real: x * RegionWidth - 2, imaginary: y * RegionWidth);
}