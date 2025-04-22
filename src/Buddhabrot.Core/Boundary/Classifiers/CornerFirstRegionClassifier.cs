using System.Buffers;
using System.Numerics;
using Buddhabrot.Core.Calculations;
using Buddhabrot.Core.Utilities;

namespace Buddhabrot.Core.Boundary.Classifiers;

public sealed class CornerFirstRegionClassifier : IRegionClassifier
{
	private readonly FixedSizeCache<RegionBatchId, BoolVector16> _cachedCorners = new(
		1024,
		defaultKey: RegionBatchId.Invalid,
		getIndex: cbi => cbi.GetHashCode1024()
	);

	private readonly BoundaryParameters _boundaryParams;

	private double RegionWidth => _boundaryParams.Divisions.RegionSideLength;

	public CornerFirstRegionClassifier(BoundaryParameters boundaryParams) => _boundaryParams = boundaryParams;

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

	public VisitedRegionType ClassifyRegion(RegionId region)
	{
		int numCorners = 0;

		numCorners += IsCornerInSet(region.LowerLeftCorner());
		numCorners += IsCornerInSet(region.LowerRightCorner());
		numCorners += IsCornerInSet(region.UpperLeftCorner());
		numCorners += IsCornerInSet(region.UpperRightCorner());

		return numCorners switch
		{
			0 => CheckRegionForFilaments(region),
			4 => VisitedRegionType.Rejected,
			_ => VisitedRegionType.Border,
		};
	}

	public (VisitedRegionType, string) DescribeRegion(RegionId region)
	{
		int numCorners = 0;

		numCorners += IsCornerInSet(region.LowerLeftCorner());
		numCorners += IsCornerInSet(region.LowerRightCorner());
		numCorners += IsCornerInSet(region.UpperLeftCorner());
		numCorners += IsCornerInSet(region.UpperRightCorner());

		return numCorners switch
		{
			0 => DescribeCheckRegionForFilaments(region),
			4 => (VisitedRegionType.Rejected, $"Corners: {numCorners}"),
			_ => (VisitedRegionType.Border, $"Corners: {numCorners}"),
		};
	}

	private VisitedRegionType CheckRegionForFilaments(RegionId region)
	{
		int numInSet = 0;
		int numFilament = 0;

		Parallel.For(
			0,
			4,
			i =>
			{
				var (iterations, distance) = ScalarKernel.FindExteriorDistance(
					ToComplex(region.X + 0.25 + (i % 2) * 0.5, region.Y + 0.25 + ((i >> 1) % 2) * 0.5),
					_boundaryParams.MaxIterations
				);

				if (iterations.IsInfinite)
				{
					Interlocked.Increment(ref numInSet);
				}
				else if (distance <= (RegionWidth / 4))
				{
					Interlocked.Increment(ref numFilament);
				}
			}
		);

		return (numInSet, numFilament) switch
		{
			(0, 0) => VisitedRegionType.Rejected,
			(4, _) => VisitedRegionType.Border,
			(_, _) => VisitedRegionType.Filament,
		};
	}

	private (VisitedRegionType, string) DescribeCheckRegionForFilaments(RegionId region)
	{
		int numInSet = 0;
		int numClose = 0;

		Parallel.For(
			0,
			4,
			i =>
			{
				var (iterations, distance) = ScalarKernel.FindExteriorDistance(
					ToComplex(region.X + 0.25 + (i % 2) * 0.5, region.Y + 0.25 + ((i >> 1) % 2) * 0.5),
					_boundaryParams.MaxIterations
				);

				if (iterations.IsInfinite)
				{
					Interlocked.Increment(ref numInSet);
				}
				else if (distance <= (RegionWidth / 4))
				{
					Interlocked.Increment(ref numClose);
				}
			}
		);

		return (numInSet, numClose) switch
		{
			(0, 0) => (VisitedRegionType.Rejected, "In Set: 0, Close: 0"),
			(4, _) => (VisitedRegionType.Border, $"In Set: {numInSet}, Close: {numClose}"),
			(_, _) => (VisitedRegionType.Filament, $"In Set: {numInSet}, Close: {numClose}"),
		};
	}

	private BoolVector16 ComputeCornerBatch(RegionBatchId id)
	{
		var inSet = ArrayPool<bool>.Shared.Rent(RegionBatchId.CornerArea);

		var bottomLeftCorner = id.GetBottomLeftCorner();

		Parallel.For(
			0,
			RegionBatchId.CornerArea,
			i =>
			{
				inSet[i] = ScalarKernel
					.FindEscapeTime(
						ToComplex(
							bottomLeftCorner + new Offset(i % RegionBatchId.CornerWidth, i / RegionBatchId.CornerWidth)
						),
						_boundaryParams.MaxIterations
					)
					.IsInfinite;
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

		ArrayPool<bool>.Shared.Return(inSet);

		return batch;
	}

	private Complex ToComplex(CornerId id) => ToComplex(id.X, id.Y);

	private Complex ToComplex(double x, double y) => new(real: x * RegionWidth - 2, imaginary: y * RegionWidth);
}
