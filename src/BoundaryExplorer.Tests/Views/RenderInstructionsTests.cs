using Avalonia;
using BoundaryExplorer.Views;
using Rectangle = System.Drawing.Rectangle;

namespace BoundaryExplorer.Tests.Views;

public class RenderInstructionsTests
{
	[Fact]
	public void ShouldIncludeEntireAreaIfInvalidatingEverything()
	{
		var inst = RenderInstructions.Everything(new PixelSize(100, 100));

		inst.PasteFrontBuffer.ShouldBeFalse();

		var rects = inst.GetDirtyRectangles().ToList();
		rects.Count.ShouldBe(1);
		rects.ShouldContain(new Rectangle(0, 0, 100, 100));
	}

	public sealed record ResizeTestCase(
		string Description,
		PixelSize OldSize,
		PixelSize NewSize,
		IEnumerable<Rectangle> DirtyRects
	)
	{
		public override string ToString() => Description;
	}

	public static IEnumerable<object[]> ResizeTestCases()
	{
		yield return
		[
			new ResizeTestCase(
				"Smaller",
				OldSize: new PixelSize(100, 100),
				NewSize: new PixelSize(50, 50),
				DirtyRects: []
			),
		];
		yield return
		[
			new ResizeTestCase(
				"Wider",
				OldSize: new PixelSize(100, 100),
				NewSize: new PixelSize(150, 100),
				DirtyRects: [new Rectangle(100, 0, 50, 100)]
			),
		];
		yield return
		[
			new ResizeTestCase(
				"Taller",
				OldSize: new PixelSize(100, 100),
				NewSize: new PixelSize(100, 150),
				DirtyRects: [new Rectangle(0, 100, 100, 50)]
			),
		];
		yield return
		[
			new ResizeTestCase(
				"Bigger",
				OldSize: new PixelSize(100, 100),
				NewSize: new PixelSize(150, 150),
				DirtyRects: [new Rectangle(100, 0, 50, 150), new Rectangle(0, 100, 100, 50)]
			),
		];
	}

	[Theory]
	[MemberData(nameof(ResizeTestCases))]
	public void ShouldComputeInstructionsForResizing(ResizeTestCase testCase)
	{
		var inst = RenderInstructions.Resized(testCase.OldSize, testCase.NewSize);

		inst.PasteFrontBuffer.ShouldBeTrue();

		inst.GetDirtyRectangles().ShouldBe(testCase.DirtyRects, ignoreOrder: true);
	}

	public sealed record MoveTestCase(
		string Description,
		PixelSize PixelSize,
		PixelVector Offset,
		Rect SourceRect,
		Rect DestRect,
		IEnumerable<Rectangle> DirtyRects
	)
	{
		public override string ToString() => Description;
	}

	public static IEnumerable<object[]> MoveTestCases()
	{
		yield return
		[
			new MoveTestCase(
				"Left",
				PixelSize: new PixelSize(100, 100),
				Offset: new PixelVector(-25, 0),
				SourceRect: new Rect(25, 0, 75, 100),
				DestRect: new Rect(0, 0, 75, 100),
				DirtyRects: [new Rectangle(75, 0, 25, 100)]
			),
		];
		yield return
		[
			new MoveTestCase(
				"Right",
				PixelSize: new PixelSize(100, 100),
				Offset: new PixelVector(25, 0),
				SourceRect: new Rect(0, 0, 75, 100),
				DestRect: new Rect(25, 0, 75, 100),
				DirtyRects: [new Rectangle(0, 0, 25, 100)]
			),
		];
		yield return
		[
			new MoveTestCase(
				"Up",
				PixelSize: new PixelSize(100, 100),
				Offset: new PixelVector(0, -25),
				SourceRect: new Rect(0, 25, 100, 75),
				DestRect: new Rect(0, 0, 100, 75),
				DirtyRects: [new Rectangle(0, 75, 100, 25)]
			),
		];
		yield return
		[
			new MoveTestCase(
				"Down",
				PixelSize: new PixelSize(100, 100),
				Offset: new PixelVector(0, 25),
				SourceRect: new Rect(0, 0, 100, 75),
				DestRect: new Rect(0, 25, 100, 75),
				DirtyRects: [new Rectangle(0, 0, 100, 25)]
			),
		];
		yield return
		[
			new MoveTestCase(
				"Up Left",
				PixelSize: new PixelSize(100, 100),
				Offset: new PixelVector(-25, -25),
				SourceRect: new Rect(25, 25, 75, 75),
				DestRect: new Rect(0, 0, 75, 75),
				DirtyRects: [new Rectangle(75, 0, 25, 100), new Rectangle(0, 75, 75, 25)]
			),
		];
		yield return
		[
			new MoveTestCase(
				"Up Right",
				PixelSize: new PixelSize(100, 100),
				Offset: new PixelVector(25, -25),
				SourceRect: new Rect(0, 25, 75, 75),
				DestRect: new Rect(25, 0, 75, 75),
				DirtyRects: [new Rectangle(0, 0, 25, 100), new Rectangle(25, 75, 75, 25)]
			),
		];
		yield return
		[
			new MoveTestCase(
				"Down Left",
				PixelSize: new PixelSize(100, 100),
				Offset: new PixelVector(-25, 25),
				SourceRect: new Rect(25, 0, 75, 75),
				DestRect: new Rect(0, 25, 75, 75),
				DirtyRects: [new Rectangle(75, 0, 25, 100), new Rectangle(0, 0, 75, 25)]
			),
		];
		yield return
		[
			new MoveTestCase(
				"Down Right",
				PixelSize: new PixelSize(100, 100),
				Offset: new PixelVector(25, 25),
				SourceRect: new Rect(0, 0, 75, 75),
				DestRect: new Rect(25, 25, 75, 75),
				DirtyRects: [new Rectangle(0, 0, 25, 100), new Rectangle(25, 0, 75, 25)]
			),
		];
	}

	[Theory]
	[MemberData(nameof(MoveTestCases))]
	public void ShouldComputeInstructionsForMoving(MoveTestCase testCase)
	{
		var inst = RenderInstructions.Moved(testCase.PixelSize, testCase.Offset);

		inst.PasteFrontBuffer.ShouldBeTrue();
		inst.SourceRect.ShouldBe(testCase.SourceRect);
		inst.DestRect.ShouldBe(testCase.DestRect);

		inst.GetDirtyRectangles().ShouldBe(testCase.DirtyRects, ignoreOrder: true);
	}
}
