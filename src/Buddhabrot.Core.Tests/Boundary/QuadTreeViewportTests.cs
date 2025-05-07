using System.Drawing;
using Buddhabrot.Core.Boundary;

namespace Buddhabrot.Core.Tests.Boundary;

public class QuadTreeViewportTests
{
	[Fact]
	public void ShouldGetExpectedQuadrants()
	{
		var v = new QuadTreeViewport(0, 0, 1);

		v.NW.ShouldBe(new QuadTreeViewport(0, 0, 0));
		v.NE.ShouldBe(new QuadTreeViewport(1, 0, 0));
		v.SE.ShouldBe(new QuadTreeViewport(1, 1, 0));
		v.SW.ShouldBe(new QuadTreeViewport(0, 1, 0));
	}

	public static IEnumerable<object[]> IntersectionData()
	{
		yield return
		[
			new Rectangle(new Point(10, 10), new Size(10, 10)),
			new Rectangle(new Point(10, 10), new Size(-8, -8)),
		];
		yield return [new Rectangle(new Point(0, 0), new Size(10, 10)), new Rectangle(new Point(0, 0), new Size(2, 2))];
	}

	[Theory]
	[MemberData(nameof(IntersectionData))]
	public void ShouldIntersectWithRectangle(Rectangle rect, Rectangle expected)
	{
		var v = new QuadTreeViewport(-2, -2, 2);
		v.IntersectWith(rect).ShouldBe(expected);
	}

	[Fact]
	public void ShouldCalculateCenteredSquareInArea()
	{
		QuadTreeViewport.GetLargestCenteredSquareInside(10, 12).ShouldBe(new QuadTreeViewport(1, 2, 3));
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
				new QuadTreeViewport(X: 1, Y: 1, Scale: 3),
				Width: 10,
				Height: 10,
				ExpectedResult: new QuadTreeViewport(X: 3, Y: 3, Scale: 2)
			),
		];
		yield return
		[
			new ZoomOutTestCase(
				"Offset - Top Left",
				new QuadTreeViewport(X: 1, Y: 1, Scale: 3),
				Width: 20,
				Height: 20,
				ExpectedResult: new QuadTreeViewport(X: 5, Y: 5, Scale: 2)
			),
		];
		yield return
		[
			new ZoomOutTestCase(
				"Offset - Top Right",
				new QuadTreeViewport(X: 11, Y: 1, Scale: 3),
				Width: 20,
				Height: 20,
				ExpectedResult: new QuadTreeViewport(X: 11, Y: 5, Scale: 2)
			),
		];
	}

	[Theory]
	[MemberData(nameof(ZoomOutData))]
	public void ShouldZoomOutCorrectly(ZoomOutTestCase testCase) =>
		testCase.Viewport.ZoomOut(width: testCase.Width, height: testCase.Height).ShouldBe(testCase.ExpectedResult);
}
