using System.Drawing;
using System.Numerics;
using SkiaSharp;

namespace Buddhabrot.Core.Tests;

public sealed class ComplexViewportTests
{
	[Fact]
	public void ShouldFigureOutMiddleOfSquareArea()
	{
		var viewPort = ComplexViewport.FromLogicalArea(new ComplexArea(new Interval(-1, 1), new Interval(-1, 1)), 101);

		var middle = viewPort.GetPosition(new Complex());

		middle.ShouldBe(new SKPointI(50, 50));
	}

	[Fact]
	public void ShouldUseTopLeftAsPositionOrigin()
	{
		var viewPort = ComplexViewport.FromLogicalArea(new ComplexArea(new Interval(-1, 1), new Interval(-1, 1)), 101);

		var topLeft = viewPort.GetPosition(new Complex(-1, 1));

		topLeft.ShouldBe(new SKPointI(0, 0));
	}

	[Fact]
	public void ShouldRoundTripPositions()
	{
		var viewPort = ComplexViewport.FromLogicalArea(new ComplexArea(new Interval(-1, 1), new Interval(-1, 1)), 101);

		var c = new Complex(-1, 1);

		var roundTripped = viewPort.GetComplex(viewPort.GetPosition(c));
		roundTripped.ShouldBe(c);
	}

	[Fact]
	public void ShouldRoundTripComplexNumbers()
	{
		var viewPort = ComplexViewport.FromLogicalArea(new ComplexArea(new Interval(-1, 1), new Interval(-1, 1)), 101);

		var point = new SKPointI(-1, 1);

		var roundTripped = viewPort.GetPosition(viewPort.GetComplex(point));
		roundTripped.ShouldBe(point);
	}

	[Fact]
	public void ShouldDetermineHeight()
	{
		var viewPort = ComplexViewport.FromLogicalArea(new ComplexArea(new Interval(-1, 1), new Interval(-1, 1)), 101);

		viewPort.Resolution.Height.ShouldBe(101);
	}

	[Fact]
	public void ShouldDetermineImaginaryMagnitude()
	{
		var viewPort = ComplexViewport.FromResolution(new SKSizeI(100, 100), new Complex(0, 0), realMagnitude: 2);

		viewPort.LogicalArea.Width.ShouldBe(2);
	}

	public sealed record ResolutionTestCase(
		string Name,
		SKSizeI Resolution,
		SKPointI OriginOffset,
		double PixelSize,
		ComplexArea ExpectedArea
	)
	{
		public override string ToString() => Name;
	}

	public static IEnumerable<object[]> GetOverlapData()
	{
		yield return
		[
			new ResolutionTestCase(
				"Center of Square 1:1",
				new SKSizeI(100, 100),
				new SKPointI(0, 0),
				1,
				ExpectedArea: new ComplexArea(new Interval(0, 100), new Interval(-100, 0))
			),
		];
		yield return
		[
			new ResolutionTestCase(
				"Center of Square 1:0.1",
				new SKSizeI(100, 100),
				new SKPointI(0, 0),
				0.1,
				ExpectedArea: new ComplexArea(new Interval(0, 10), new Interval(-10, 0))
			),
		];
		yield return
		[
			new ResolutionTestCase(
				"Offset Right",
				new SKSizeI(100, 100),
				new SKPointI(50, 0),
				0.1,
				ExpectedArea: new ComplexArea(new Interval(-5, 5), new Interval(-10, 0))
			),
		];
		yield return
		[
			new ResolutionTestCase(
				"Offset Up Imag Axis",
				new SKSizeI(100, 100),
				new SKPointI(0, 50),
				0.1,
				ExpectedArea: new ComplexArea(new Interval(0, 10), new Interval(-5, 5))
			),
		];
	}

	[Theory]
	[MemberData(nameof(GetOverlapData))]
	public void ShouldConstructFromResolution(ResolutionTestCase testCase)
	{
		var viewPort = ComplexViewport.FromResolution(testCase.Resolution, testCase.OriginOffset, testCase.PixelSize);
		viewPort.LogicalArea.ShouldBe(testCase.ExpectedArea);
	}
}
