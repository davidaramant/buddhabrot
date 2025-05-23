using Buddhabrot.Core.Boundary;
using Buddhabrot.Core.DataStorage;

namespace Buddhabrot.Core.Tests.DataStorage;

public class BoundaryDataSetTests
{
	[Fact]
	public void ShouldHandleWeirdIterationCount()
	{
		var iterations = 1_000_001;
		var bds = BoundaryDataSet.FromBoundary(new BoundaryParameters(new AreaDivisions(1), iterations));
		var roundTripped = BoundaryDataSet.FromDescription(bds.Description);
		roundTripped.Parameters.MaxIterations.ShouldBe(iterations);
	}

	[Fact]
	public void ShouldDeserializeNormalBoundaryParameters()
	{
		var bds = BoundaryDataSet.FromDescription("v8_i1M");
		bds.IsDiff.ShouldBeFalse();
		bds.Parameters.Divisions.VerticalPower.ShouldBe(8);
		bds.Parameters.MaxIterations.ShouldBe(1_000_000);
	}

	[Fact]
	public void ShouldDeserializeDiff()
	{
		var bds = BoundaryDataSet.FromDescription("Diff - v9_i1M - v8_i100k");
		bds.IsDiff.ShouldBeTrue();
		bds.Parameters.Divisions.VerticalPower.ShouldBe(9);
		bds.Parameters.MaxIterations.ShouldBe(1_000_000);
	}

	[Fact]
	public void ShouldCompareDataSetsCorrectly()
	{
		static BoundaryParameters MakeParams(int verticalPower, int maxIterations) =>
			new BoundaryParameters(new AreaDivisions(verticalPower), maxIterations);

		var sets = new[]
		{
			BoundaryDataSet.FromDiff(MakeParams(11, 100), MakeParams(12, 200)),
			BoundaryDataSet.FromBoundary(MakeParams(10, 100_000)),
			BoundaryDataSet.FromBoundary(MakeParams(9, 100_000)),
			BoundaryDataSet.FromDiff(MakeParams(13, 100_000), MakeParams(12, 100_000_000)),
			BoundaryDataSet.FromBoundary(MakeParams(11, 100_000)),
			BoundaryDataSet.FromBoundary(MakeParams(10, 10_000)),
		};

		var expectedOrder = new[]
		{
			BoundaryDataSet.FromBoundary(MakeParams(11, 100_000)),
			BoundaryDataSet.FromBoundary(MakeParams(10, 100_000)),
			BoundaryDataSet.FromBoundary(MakeParams(10, 10_000)),
			BoundaryDataSet.FromBoundary(MakeParams(9, 100_000)),
			BoundaryDataSet.FromDiff(MakeParams(13, 100_000), MakeParams(12, 100_000_000)),
			BoundaryDataSet.FromDiff(MakeParams(11, 100), MakeParams(12, 200)),
		};

		var ordered = sets.OrderBy(s => s).ToList();

		ordered.ShouldBe(expectedOrder);
	}
}
