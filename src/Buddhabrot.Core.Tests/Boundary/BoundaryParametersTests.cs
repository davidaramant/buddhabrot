using Buddhabrot.Core.Boundary;

namespace Buddhabrot.Core.Tests.Boundary;

public class BoundaryParametersTests
{
    [Theory]
    [InlineData(1_000)]
    [InlineData(1_000_000)]
    public void ShouldRoundtrip(int iterations)
    {
        var bp = new BoundaryParameters(new AreaDivisions(10), iterations);
        var roundTripped = BoundaryParameters.FromDescription(bp.Description);
        roundTripped.MaxIterations.Should().Be(iterations);
    }

    [Fact]
    public void ShouldDeserializeFromOldFormat()
    {
        var bp = BoundaryParameters.FromDescription("v8_i1,000,000");
        bp.Divisions.VerticalPower.Should().Be(8);
        bp.MaxIterations.Should().Be(1_000_000);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Some string")]
    public void ShouldRoundTripMetadata(string metadata)
    {
        var bp = new BoundaryParameters(new AreaDivisions(1), 10, metadata);
        var roundTripped = BoundaryParameters.FromDescription(bp.Description);
        roundTripped.Metadata.Should().Be(metadata);
    }
}