using System.Buffers;
using System.Numerics;
using Buddhabrot.Core.Calculations;
using Buddhabrot.Core.Utilities;

namespace Buddhabrot.Core.Boundary.Corners;

public sealed class RegionInspector : IRegionClassifier
{
    private readonly FixedSizeCache<RegionBatchId, BoolVector16> _cachedCorners = new(64,
        defaultKey: RegionBatchId.Invalid,
        getIndex: cbi => cbi.GetHashCode64());

    private readonly BoundaryParameters _boundaryParams;

    private double RegionWidth => _boundaryParams.Divisions.RegionSideLength;

    public RegionInspector(BoundaryParameters boundaryParams) => _boundaryParams = boundaryParams;

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

    private (int InSet, int Close) SampleRegionInterior(RegionId region)
    {
        var centers = ArrayPool<Complex>.Shared.Rent(16);

        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                centers[y * 4 + x] = ToComplex(region.X + x * 0.25 + 0.125, region.Y + y * 0.25 + 0.125);
            }
        }

        int inSet = 0;
        int close = 0;

        Parallel.For(0, centers.Length, i =>
        {
            var (iterations, distance) = ScalarKernel.FindExteriorDistance(centers[i], _boundaryParams.MaxIterations);

            if (iterations.IsInfinite)
            {
                Interlocked.Increment(ref inSet);
            }
            else if (distance <= (RegionWidth / 8))
            {
                Interlocked.Increment(ref close);
            }
        });

        ArrayPool<Complex>.Shared.Return(centers);

        return (inSet, close);
    }

    public (int CornersInSet, int InteriorsInSet, int InteriorsClose) InspectRegion(RegionId region)
    {
        int cornersInSet = 0;

        void CheckCorner(CornerId corner)
        {
            if (IsCornerInSet(corner))
            {
                cornersInSet++;
            }
        }

        CheckCorner(region.LowerLeftCorner());
        CheckCorner(region.LowerRightCorner());
        CheckCorner(region.UpperLeftCorner());
        CheckCorner(region.UpperRightCorner());

        var (interiorInSet, interiorClose) = SampleRegionInterior(region);

        return (cornersInSet, interiorInSet, interiorClose);
    }

    public VisitedRegionType ClassifyRegion(int cornersInSet, int interiorsInSet, int interiorsClose)
    {
        return (cornersInSet, interiorsInSet, interiorsClose) switch
        {
            // Totally empty
            (0, 0, 0) => VisitedRegionType.Rejected,

            // Inside set
            (4, 16, _) => VisitedRegionType.Rejected,

            {cornersInSet: > 0, interiorsInSet: 0} => VisitedRegionType.Filament,
            
            {interiorsInSet: > 0, interiorsClose: > 1} => VisitedRegionType.Border,

            _ => VisitedRegionType.Filament
        };
    }


    // TODO: Make this use Inspect & Classify once those are good
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

    private VisitedRegionType CheckRegionForFilaments(RegionId region)
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
            var (iterations, distance) = ScalarKernel.FindExteriorDistance(centers[i], _boundaryParams.MaxIterations);

            if (iterations.IsInfinite)
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