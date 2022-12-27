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
}