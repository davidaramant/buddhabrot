using System.Buffers;
using System.Numerics;
using Buddhabrot.Core.Calculations;
using Buddhabrot.Core.Utilities;

namespace Buddhabrot.Core.Boundary.Classifiers;

public sealed class CornerFirstRegionClassifier : IRegionClassifier
{
	private readonly FixedSizeCache<RegionBatchId, BoolVector16> _cachedCorners =
		new(64, defaultKey: RegionBatchId.Invalid, getIndex: cbi => cbi.GetHashCode64());

	private readonly BoundaryParameters _boundaryParams;

	private double RegionWidth => _boundaryParams.Divisions.RegionSideLength;

	public CornerFirstRegionClassifier(BoundaryParameters boundaryParams) => _boundaryParams = boundaryParams;

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

	public (VisitedRegionType, string) DescribeRegion(RegionId region)
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
			0 => DescribeCheckRegionForFilaments(region),
			4 => (VisitedRegionType.Rejected, $"Corners: {numCorners}"),
			_ => (VisitedRegionType.Border, $"Corners: {numCorners}"),
		};
	}

	private VisitedRegionType CheckRegionForFilaments(RegionId region)
	{
		var centers = ArrayPool<Complex>.Shared.Rent(4);

		centers[0] = ToComplex(region.X + 0.25, region.Y + 0.25);
		centers[1] = ToComplex(region.X + 0.75, region.Y + 0.25);
		centers[2] = ToComplex(region.X + 0.25, region.Y + 0.75);
		centers[3] = ToComplex(region.X + 0.75, region.Y + 0.75);

		int numInSet = 0;
		int numFilament = 0;

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
					Interlocked.Increment(ref numInSet);
				}
				else if (distance <= (RegionWidth / 4))
				{
					Interlocked.Increment(ref numFilament);
				}
			}
		);

		ArrayPool<Complex>.Shared.Return(centers);

		return (numInSet, numFilament) switch
		{
			(0, 0) => VisitedRegionType.Rejected,
			(4, _) => VisitedRegionType.Border,
			(_, _) => VisitedRegionType.Filament,
		};
	}

	private (VisitedRegionType, string) DescribeCheckRegionForFilaments(RegionId region)
	{
		var centers = ArrayPool<Complex>.Shared.Rent(4);

		centers[0] = ToComplex(region.X + 0.25, region.Y + 0.25);
		centers[1] = ToComplex(region.X + 0.75, region.Y + 0.25);
		centers[2] = ToComplex(region.X + 0.25, region.Y + 0.75);
		centers[3] = ToComplex(region.X + 0.75, region.Y + 0.75);

		int numInSet = 0;
		int numClose = 0;

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
					Interlocked.Increment(ref numInSet);
				}
				else if (distance <= (RegionWidth / 4))
				{
					Interlocked.Increment(ref numClose);
				}
			}
		);

		ArrayPool<Complex>.Shared.Return(centers);

		return (numInSet, numClose) switch
		{
			(0, 0) => (VisitedRegionType.Rejected, "In Set: 0, Close: 0"),
			(4, _) => (VisitedRegionType.Border, $"In Set: {numInSet}, Close: {numClose}"),
			(_, _) => (VisitedRegionType.Filament, $"In Set: {numInSet}, Close: {numClose}"),
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
