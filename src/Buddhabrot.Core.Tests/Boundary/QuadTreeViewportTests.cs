using Buddhabrot.Core.Boundary;
using SkiaSharp;

namespace Buddhabrot.Core.Tests.Boundary;

public class QuadTreeViewportTests
{
	[Fact]
	public void ShouldGetExpectedQuadrants()
	{
		var v = new QuadTreeViewport(SKRectI.Create(new SKPointI(0, 0), new SKSizeI(20, 20)), 1);

		v.NW.ShouldBe(new QuadTreeViewport(SKRectI.Create(new SKPointI(0, 0), new SKSizeI(10, 10)), 0));
		v.NE.ShouldBe(new QuadTreeViewport(SKRectI.Create(new SKPointI(1, 0), new SKSizeI(10, 10)), 0));
		v.SE.ShouldBe(new QuadTreeViewport(SKRectI.Create(new SKPointI(1, 1), new SKSizeI(10, 10)), 0));
		v.SW.ShouldBe(new QuadTreeViewport(SKRectI.Create(new SKPointI(0, 1), new SKSizeI(10, 10)), 0));
	}

	public static IEnumerable<object[]> IntersectionData()
	{
		yield return
		[
			SKRectI.Create(new SKPointI(10, 10), new SKSizeI(10, 10)),
			SKRectI.Create(new SKPointI(10, 10), new SKSizeI(-8, -8)),
		];
		yield return
		[
			SKRectI.Create(new SKPointI(0, 0), new SKSizeI(10, 10)),
			SKRectI.Create(new SKPointI(0, 0), new SKSizeI(2, 2)),
		];
	}

	[Theory]
	[MemberData(nameof(IntersectionData))]
	public void ShouldIntersectWithRectangle(SKRectI rect, SKRectI expected)
	{
		var v = new QuadTreeViewport(SKRectI.Create(new SKPointI(-2, -2), new SKSizeI(20, 20)), 2);
		v.IntersectWith(rect).ShouldBe(expected);
	}

	[Fact]
	public void ShouldCalculateCenteredSquareInArea()
	{
		QuadTreeViewport
			.GetLargestCenteredSquareInside(new(10, 12))
			.ShouldBe(new QuadTreeViewport(SKRectI.Create(new SKPointI(1, 2), new(10, 12)), 3));
	}

	public sealed record ZoomOutTestCase(string Name, QuadTreeViewport Viewport, QuadTreeViewport ExpectedResult)
	{
		public override string ToString() => Name;
	}

	public static IEnumerable<object[]> ZoomOutData()
	{
		yield return
		[
			new ZoomOutTestCase(
				"Centered",
				new QuadTreeViewport(SKRectI.Create(new SKPointI(1, 1), new SKSizeI(10, 10)), Scale: 3),
				ExpectedResult: new QuadTreeViewport(SKRectI.Create(new SKPointI(3, 3), new SKSizeI(10, 10)), Scale: 2)
			),
		];
		yield return
		[
			new ZoomOutTestCase(
				"Offset - Top Left",
				new QuadTreeViewport(SKRectI.Create(new SKPointI(1, 1), new SKSizeI(20, 20)), Scale: 3),
				ExpectedResult: new QuadTreeViewport(SKRectI.Create(new SKPointI(5, 5), new SKSizeI(20, 20)), Scale: 2)
			),
		];
		yield return
		[
			new ZoomOutTestCase(
				"Offset - Top Right",
				new QuadTreeViewport(SKRectI.Create(new SKPointI(x: 11, y: 1), new SKSizeI(20, 20)), Scale: 3),
				ExpectedResult: new QuadTreeViewport(
					SKRectI.Create(new SKPointI(x: 11, y: 5), new SKSizeI(20, 20)),
					Scale: 2
				)
			),
		];
	}

	[Theory]
	[MemberData(nameof(ZoomOutData))]
	public void ShouldZoomOutCorrectly(ZoomOutTestCase testCase) =>
		testCase.Viewport.ZoomOut().ShouldBe(testCase.ExpectedResult);
}
