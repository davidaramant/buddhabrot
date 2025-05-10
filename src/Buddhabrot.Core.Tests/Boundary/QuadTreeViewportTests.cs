using Buddhabrot.Core.Boundary;
using SkiaSharp;

namespace Buddhabrot.Core.Tests.Boundary;

public class QuadTreeViewportTests
{
	[Fact]
	public void ShouldGetExpectedQuadrants()
	{
		var v = new QuadTreeViewport(new SKPointI(0, 0), 1);

		v.NW.ShouldBe(new QuadTreeViewport(new SKPointI(0, 0), 0));
		v.NE.ShouldBe(new QuadTreeViewport(new SKPointI(1, 0), 0));
		v.SE.ShouldBe(new QuadTreeViewport(new SKPointI(1, 1), 0));
		v.SW.ShouldBe(new QuadTreeViewport(new SKPointI(0, 1), 0));
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
		var v = new QuadTreeViewport(new SKPointI(-2, -2), 2);
		v.IntersectWith(rect).ShouldBe(expected);
	}

	[Fact]
	public void ShouldCalculateCenteredSquareInArea()
	{
		QuadTreeViewport.GetLargestCenteredSquareInside(10, 12).ShouldBe(new QuadTreeViewport(new SKPointI(1, 2), 3));
	}

	public sealed record ZoomOutTestCase(
		string Name,
		QuadTreeViewport Viewport,
		int Width,
		int Height,
		QuadTreeViewport ExpectedResult
	)
	{
		public override string ToString() => Name;
	}

	public static IEnumerable<object[]> ZoomOutData()
	{
		yield return
		[
			new ZoomOutTestCase(
				"Centered",
				new QuadTreeViewport(new SKPointI(1, 1), Scale: 3),
				Width: 10,
				Height: 10,
				ExpectedResult: new QuadTreeViewport(new SKPointI(3, 3), Scale: 2)
			),
		];
		yield return
		[
			new ZoomOutTestCase(
				"Offset - Top Left",
				new QuadTreeViewport(new SKPointI(1, 1), Scale: 3),
				Width: 20,
				Height: 20,
				ExpectedResult: new QuadTreeViewport(new SKPointI(5, 5), Scale: 2)
			),
		];
		yield return
		[
			new ZoomOutTestCase(
				"Offset - Top Right",
				new QuadTreeViewport(new SKPointI(x: 11, y: 1), Scale: 3),
				Width: 20,
				Height: 20,
				ExpectedResult: new QuadTreeViewport(new SKPointI(x: 11, y: 5), Scale: 2)
			),
		];
	}

	[Theory]
	[MemberData(nameof(ZoomOutData))]
	public void ShouldZoomOutCorrectly(ZoomOutTestCase testCase) =>
		testCase.Viewport.ZoomOut(width: testCase.Width, height: testCase.Height).ShouldBe(testCase.ExpectedResult);
}
