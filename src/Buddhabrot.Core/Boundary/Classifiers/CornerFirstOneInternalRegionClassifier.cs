using System.Numerics;
using System.Runtime.InteropServices;
using Buddhabrot.Core.Calculations;
using Buddhabrot.Core.Utilities;
using CommunityToolkit.HighPerformance.Helpers;

namespace Buddhabrot.Core.Boundary.Classifiers;

public sealed class CornerFirstOneInternalRegionClassifier : IRegionClassifier
{
	private readonly FixedSizeCache<RegionBatchId, BoolVector16> _cachedCorners = new(
		1024,
		defaultKey: RegionBatchId.Invalid,
		getIndex: cbi => cbi.GetHashCode1024()
	);

	private readonly BoundaryParameters _boundaryParams;

	[StructLayout(LayoutKind.Explicit, Size = 64)]
	public readonly struct PaddedBool(bool value)
	{
		[FieldOffset(0)]
		public readonly bool Value = value;

		// Remaining 63 bytes are automatically padded
	}

	private readonly PaddedBool[] _inSet = new PaddedBool[RegionBatchId.CornerArea];

	private double RegionWidth => _boundaryParams.Divisions.RegionSideLength;

	public CornerFirstOneInternalRegionClassifier(BoundaryParameters boundaryParams) =>
		_boundaryParams = boundaryParams;

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
		var (iterations, distance) = ScalarKernel.FindExteriorDistance(
			ToComplex(region.X + 0.5, region.Y + 0.5),
			_boundaryParams.MaxIterations
		);

		if (iterations.IsInfinite)
			return VisitedRegionType.Border;

		return distance <= (RegionWidth / 2) ? VisitedRegionType.Filament : VisitedRegionType.Rejected;
	}

	private (VisitedRegionType, string) DescribeCheckRegionForFilaments(RegionId region)
	{
		var (iterations, distance) = ScalarKernel.FindExteriorDistance(
			ToComplex(region.X + 0.5, region.Y + 0.5),
			_boundaryParams.MaxIterations
		);

		if (iterations.IsInfinite)
			return (VisitedRegionType.Border, "Center in set");

		return distance <= (RegionWidth / 2)
			? (VisitedRegionType.Filament, "Center close")
			: (VisitedRegionType.Rejected, "Center far");
	}

	private BoolVector16 ComputeCornerBatch(RegionBatchId id)
	{
		var bottomLeftCorner = id.GetBottomLeftCorner();

		ParallelHelper.For(
			0,
			RegionBatchId.CornerArea,
			new InSetAction(
				regionWidth: RegionWidth,
				bottomLeftCorner: bottomLeftCorner,
				inSet: _inSet,
				maxIterations: _boundaryParams.MaxIterations
			),
			minimumActionsPerThread: 1
		);

		var batch = BoolVector16.Empty;

		for (int i = 0; i < RegionBatchId.CornerArea; i++)
		{
			if (_inSet[i].Value)
			{
				batch = batch.WithBit(i);
			}
		}

		return batch;
	}

	private readonly struct InSetAction(
		double regionWidth,
		CornerId bottomLeftCorner,
		PaddedBool[] inSet,
		int maxIterations
	) : IAction
	{
		public void Invoke(int i)
		{
			inSet[i] = new(
				ScalarKernel
					.FindEscapeTime(
						ToComplex(
							bottomLeftCorner + new Offset(i % RegionBatchId.CornerWidth, i / RegionBatchId.CornerWidth)
						),
						maxIterations
					)
					.IsInfinite
			);
		}

		private Complex ToComplex(CornerId id) => ToComplex(id.X, id.Y);

		private Complex ToComplex(double x, double y) => new(real: x * regionWidth - 2, imaginary: y * regionWidth);
	}

	private Complex ToComplex(double x, double y) => new(real: x * RegionWidth - 2, imaginary: y * RegionWidth);
}
