using Buddhabrot.Core.Boundary;

namespace Buddhabrot.Core.Tests.Boundary;

public class BoundaryParametersTests
{
	[Test]
	[Arguments(1_000)]
	[Arguments(1_000_000)]
	public void ShouldRoundtrip(int iterations)
	{
		var bp = new BoundaryParameters(new AreaDivisions(10), iterations);
		var roundTripped = BoundaryParameters.FromDescription(bp.Description);
		roundTripped.MaxIterations.ShouldBe(iterations);
	}

	[Test]
	public void ShouldDeserializeFromOldFormat()
	{
		var bp = BoundaryParameters.FromDescription("v8_i1,000,000");
		bp.Divisions.VerticalPower.ShouldBe(8);
		bp.MaxIterations.ShouldBe(1_000_000);
	}

	[Test]
	[Arguments("")]
	[Arguments("Some string")]
	public void ShouldRoundTripMetadata(string metadata)
	{
		var bp = new BoundaryParameters(new AreaDivisions(1), 10, metadata);
		var roundTripped = BoundaryParameters.FromDescription(bp.Description);
		roundTripped.Metadata.ShouldBe(metadata);
	}
}