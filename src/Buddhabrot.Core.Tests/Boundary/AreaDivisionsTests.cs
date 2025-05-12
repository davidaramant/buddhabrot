using System.Numerics;
using Buddhabrot.Core.Boundary;
using Buddhabrot.Core.Boundary.Classifiers;

namespace Buddhabrot.Core.Tests.Boundary;

public class AreaDivisionsTests
{
	[Theory]
	[InlineData(3, 9)]
	[InlineData(10, 1024 + 128)]
	public void ShouldComputeRightStart(int verticalPower, int expectedX)
	{
		var divisions = new AreaDivisions(verticalPower);

		var region = divisions.RightBorderStart();
		region.X.ShouldBe(expectedX);
		region.Y.ShouldBe(0);

		var corner = region.LowerLeftCorner();
		var cornerPoint = divisions.ToComplex(corner);

		cornerPoint.ShouldBe(new Complex(0.25, 0));
	}
}
