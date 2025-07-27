using System.Numerics;
using Buddhabrot.Core.Calculations;
using Buddhabrot.Core.Utilities;
using CommunityToolkit.HighPerformance.Helpers;

namespace Buddhabrot.Core.Boundary.Classifiers;

public sealed class CornerFirstFourInternalRegionClassifier : IRegionClassifier
{
	private readonly FixedSizeCache<RegionBatchId, BoolVector16> _cachedCorners = new(
		1024,
		defaultKey: RegionBatchId.Invalid,
		getIndex: cbi => cbi.GetHashCode1024()
	);

	private readonly BoundaryParameters _boundaryParams;

	private const int FalseSharingPadding = 128;
	private const int BoolFalseSharingPadding = FalseSharingPadding / sizeof(bool);
	private const int IntFalseSharingPadding = FalseSharingPadding / sizeof(int);

	private readonly bool[] _inSet = new bool[RegionBatchId.CornerArea * BoolFalseSharingPadding];
	private readonly int[] _interiorsInSet = new int[4 * IntFalseSharingPadding];
	private readonly int[] _interiorsClose = new int[4 * IntFalseSharingPadding];

	private double RegionWidth => _boundaryParams.Divisions.RegionSideLength;

	public CornerFirstFourInternalRegionClassifier(BoundaryParameters boundaryParams) =>
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
		for (int i = 0; i < 4 * IntFalseSharingPadding; i += IntFalseSharingPadding)
		{
			_interiorsInSet[i] = 0;
			_interiorsClose[i] = 0;
		}

		ParallelHelper.For(
			0,
			4,
			new FilamentCheckAction(
				region,
				RegionWidth,
				_interiorsInSet,
				_interiorsClose,
				_boundaryParams.MaxIterations
			)
		);

		int numInSet = 0;
		int numFilament = 0;

		for (int i = 0; i < 4 * IntFalseSharingPadding; i += IntFalseSharingPadding)
		{
			numInSet += _interiorsInSet[i];
			numFilament += _interiorsClose[i];
		}

		return (numInSet, numFilament) switch
		{
			(0, 0) => VisitedRegionType.Rejected,
			(4, _) => VisitedRegionType.Border,
			(_, _) => VisitedRegionType.Filament,
		};
	}

	private readonly struct FilamentCheckAction(
		RegionId region,
		double regionWidth,
		int[] inSet,
		int[] close,
		int maxIterations
	) : IAction
	{
		public void Invoke(int i)
		{
			var (iterations, distance) = ScalarKernel.FindExteriorDistance(
				ToComplex(region.X + 0.25 + (i % 2) * 0.5, region.Y + 0.25 + ((i >> 1) % 2) * 0.5),
				maxIterations
			);

			if (iterations.IsInfinite)
			{
				inSet[i * IntFalseSharingPadding] = 1;
			}
			else if (distance <= (regionWidth / 4))
			{
				close[i * IntFalseSharingPadding] = 1;
			}
		}

		private Complex ToComplex(double x, double y) => new(real: x * regionWidth - 2, imaginary: y * regionWidth);
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
		var bottomLeftCorner = id.GetBottomLeftCorner();

		ParallelHelper.For(
			0,
			RegionBatchId.CornerArea,
			new InSetAction(
				regionWidth: RegionWidth,
				bottomLeftCorner: bottomLeftCorner,
				inSet: _inSet,
				maxIterations: _boundaryParams.MaxIterations
			)
		);

		var batch = BoolVector16.Empty;

		for (int i = 0; i < RegionBatchId.CornerArea; i++)
		{
			if (_inSet[i * BoolFalseSharingPadding])
			{
				batch = batch.WithBit(i);
			}
		}

		return batch;
	}

	private readonly struct InSetAction(double regionWidth, CornerId bottomLeftCorner, bool[] inSet, int maxIterations)
		: IAction
	{
		public void Invoke(int i)
		{
			inSet[i * BoolFalseSharingPadding] = ScalarKernel
				.FindEscapeTime(
					ToComplex(
						bottomLeftCorner + new Offset(i % RegionBatchId.CornerWidth, i / RegionBatchId.CornerWidth)
					),
					maxIterations
				)
				.IsInfinite;
		}

		private Complex ToComplex(CornerId id) => ToComplex(id.X, id.Y);

		private Complex ToComplex(double x, double y) => new(real: x * regionWidth - 2, imaginary: y * regionWidth);
	}

	private Complex ToComplex(double x, double y) => new(real: x * RegionWidth - 2, imaginary: y * RegionWidth);
}
