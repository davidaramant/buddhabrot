using Buddhabrot.Core.Boundary;
using Buddhabrot.Core.DataStorage;

namespace Buddhabrot.Core.Tests.DataStorage;

public class BoundaryDataSetTests
{
    [Fact]
    public void ShouldDeserializeNormalBoundaryParameters()
    {
        var bds = BoundaryDataSet.FromDescription("v8_i1M");
        bds.IsDiff.Should().BeFalse();
        bds.Parameters.Divisions.VerticalPower.Should().Be(8);
        bds.Parameters.MaxIterations.Should().Be(1_000_000);
    }

    [Fact]
    public void ShouldDeserializeDiff()
    {
        var bds = BoundaryDataSet.FromDescription("Diff - v9_i1M - v8_i100k");
        bds.IsDiff.Should().BeTrue();
        bds.Parameters.Divisions.VerticalPower.Should().Be(9);
        bds.Parameters.MaxIterations.Should().Be(1_000_000);
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

        ordered.Should().ContainInOrder(expectedOrder);
    }
}