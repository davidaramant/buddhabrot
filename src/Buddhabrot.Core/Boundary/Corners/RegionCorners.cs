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
        var centers = ArrayPool<Complex>.Shared.Rent(4);

        centers[0] = ToComplex(region.X + 0.25, region.Y + 0.25);
        centers[1] = ToComplex(region.X + 0.75, region.Y + 0.25);
        centers[2] = ToComplex(region.X + 0.25, region.Y + 0.75);
        centers[3] = ToComplex(region.X + 0.75, region.Y + 0.75);

        int numBorder = 0;
        int numFilament = 0;

        Parallel.For(0, 4, i =>
        {
            var distance = ScalarKernel.FindExteriorDistance(centers[i], _boundaryParams.MaxIterations);

            // TODO: Make FindExteriorDistance return a tuple of (iterations,distance) to avoid this comparison
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (distance == double.MaxValue)
            {
                Interlocked.Increment(ref numBorder);
            }
            else if (distance <= (RegionWidth / 4))
            {
                Interlocked.Increment(ref numFilament);
            }
        });

        ArrayPool<Complex>.Shared.Return(centers);

        return (numBorder, numFilament) switch
        {
            (0, 0) => VisitedRegionType.Rejected,
            (4, _) => VisitedRegionType.Border,
            (_, _) => VisitedRegionType.Filament,
        };
    }

    public VisitedRegionType ClassifyRegion(RegionId region)
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
            0 => CheckRegionForFilaments(region),
            4 => VisitedRegionType.Rejected,
            _ => VisitedRegionType.Border,
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

    private Complex ToComplex(CornerId id) => ToComplex(id.X, id.Y);
    private Complex ToComplex(double x, double y) => new(real: x * RegionWidth - 2, imaginary: y * RegionWidth);
}