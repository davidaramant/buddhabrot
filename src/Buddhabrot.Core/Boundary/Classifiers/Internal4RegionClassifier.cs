using System.Buffers;
using System.Numerics;
using Buddhabrot.Core.Calculations;
using Buddhabrot.Core.Utilities;

namespace Buddhabrot.Core.Boundary.Classifiers;

public sealed class Interior4RegionClassifier : IRegionClassifier
{
	private readonly FixedSizeCache<RegionBatchId, BoolVector16> _cachedCorners = new(
		64,
		defaultKey: RegionBatchId.Invalid,
		getIndex: cbi => cbi.GetHashCode64()
	);

	private readonly BoundaryParameters _boundaryParams;

	private double RegionWidth => _boundaryParams.Divisions.RegionSideLength;

	public Interior4RegionClassifier(BoundaryParameters boundaryParams) => _boundaryParams = boundaryParams;

	private int IsCornerInSet(CornerId corner)
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
		var centers = ArrayPool<Complex>.Shared.Rent(4);

		for (int y = 0; y < 2; y++)
		{
			for (int x = 0; x < 2; x++)
			{
				centers[y * 2 + x] = ToComplex(region.X + x * 0.5 + 0.25, region.Y + y * 0.5 + 0.25);
			}
		}

		int inSet = 0;
		int close = 0;

		Parallel.For(
			0,
			4,
			i =>
			{
				var (iterations, distance) = ScalarKernel.FindExteriorDistance(
					centers[i],
					_boundaryParams.MaxIterations
				);

				if (iterations.IsInfinite)
				{
					Interlocked.Increment(ref inSet);
				}
				else if (distance <= (RegionWidth / 2))
				{
					Interlocked.Increment(ref close);
				}
			}
		);

		ArrayPool<Complex>.Shared.Return(centers);

		return (inSet, close);
	}

	private (int CornersInSet, int InteriorsInSet, int InteriorsClose) InspectRegion(RegionId region)
	{
		int cornersInSet = 0;

		cornersInSet += IsCornerInSet(region.LowerLeftCorner());
		cornersInSet += IsCornerInSet(region.LowerRightCorner());
		cornersInSet += IsCornerInSet(region.UpperLeftCorner());
		cornersInSet += IsCornerInSet(region.UpperRightCorner());

		var (interiorInSet, interiorClose) = SampleRegionInterior(region);

		return (cornersInSet, interiorInSet, interiorClose);
	}

	private VisitedRegionType ClassifyRegion(int cornersInSet, int interiorsInSet, int interiorsClose) =>
		(cornersInSet, interiorsInSet, interiorsClose) switch
		{
			(0, 0, 0) => VisitedRegionType.Rejected, // Totally empty
			(4, 4, _) => VisitedRegionType.Rejected, // Inside set
			{ cornersInSet: > 0, interiorsInSet: 0, interiorsClose: > 2 } => VisitedRegionType.Border,
			{ cornersInSet: > 0, interiorsInSet: > 0 } => VisitedRegionType.Border,
			{ interiorsInSet: > 0, interiorsClose: > 0 } => VisitedRegionType.Border,
			_ => VisitedRegionType.Filament,
		};

	public VisitedRegionType ClassifyRegion(RegionId region)
	{
		var result = InspectRegion(region);
		return ClassifyRegion(result.CornersInSet, result.InteriorsInSet, result.InteriorsClose);
	}

	public (VisitedRegionType, string) DescribeRegion(RegionId region)
	{
		var result = InspectRegion(region);
		var description =
			$"Corners: {result.CornersInSet}, "
			+ $"Inside: {result.InteriorsInSet}, "
			+ $"Inside Close: {result.InteriorsClose} ";
		return (ClassifyRegion(result.CornersInSet, result.InteriorsInSet, result.InteriorsClose), description);
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

		Parallel.For(
			0,
			RegionBatchId.CornerArea,
			i =>
			{
				inSet[i] = ScalarKernel.FindEscapeTime(corners[i], _boundaryParams.MaxIterations).IsInfinite;
			}
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
