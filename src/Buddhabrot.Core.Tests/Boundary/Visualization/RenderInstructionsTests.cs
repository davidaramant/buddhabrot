using Buddhabrot.Core.Boundary.Visualization;
using SkiaSharp;

namespace Buddhabrot.Core.Tests.Boundary.Visualization;

public class RenderInstructionsTests
{
	[Fact]
	public void ShouldIncludeEntireAreaIfInvalidatingEverything()
	{
		var inst = RenderInstructions.Everything(new SKSizeI(100, 100));

		inst.PasteFrontBuffer.ShouldBeFalse();

		var rects = inst.GetDirtyRectangles().ToList();
		rects.Count.ShouldBe(1);
		rects.ShouldContain(SKRectI.Create(0, 0, 100, 100));
	}

	public sealed record ResizeTestCase(
		string Description,
		SKSizeI OldSize,
		SKSizeI NewSize,
		IEnumerable<SKRectI> DirtyRects
	)
	{
		public override string ToString() => Description;
	}

	public static IEnumerable<object[]> ResizeTestCases()
	{
		yield return
		[
			new ResizeTestCase("Smaller", OldSize: new SKSizeI(100, 100), NewSize: new SKSizeI(50, 50), DirtyRects: []),
		];
		yield return
		[
			new ResizeTestCase(
				"Wider",
				OldSize: new SKSizeI(100, 100),
				NewSize: new SKSizeI(150, 100),
				DirtyRects: [SKRectI.Create(100, 0, 50, 100)]
			),
		];
		yield return
		[
			new ResizeTestCase(
				"Taller",
				OldSize: new SKSizeI(100, 100),
				NewSize: new SKSizeI(100, 150),
				DirtyRects: [SKRectI.Create(0, 100, 100, 50)]
			),
		];
		yield return
		[
			new ResizeTestCase(
				"Bigger",
				OldSize: new SKSizeI(100, 100),
				NewSize: new SKSizeI(150, 150),
				DirtyRects: [SKRectI.Create(100, 0, 50, 150), SKRectI.Create(0, 100, 100, 50)]
			),
		];
	}

	[Theory]
	[MemberData(nameof(ResizeTestCases))]
	public void ShouldComputeInstructionsForResizing(ResizeTestCase testCase)
	{
		var inst = RenderInstructions.Everything(testCase.OldSize).Resize(testCase.NewSize);

		inst.PasteFrontBuffer.ShouldBeTrue();

		inst.GetDirtyRectangles().ShouldBe(testCase.DirtyRects, ignoreOrder: true);
	}

	public sealed record MoveTestCase(
		string Description,
		SKSizeI Size,
		PositionOffset Offset,
		SKRectI SourceRect,
		SKRectI DestRect,
		IEnumerable<SKRectI> DirtyRects
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
				Size: new SKSizeI(100, 100),
				Offset: new PositionOffset(-25, 0),
				SourceRect: SKRectI.Create(25, 0, 75, 100),
				DestRect: SKRectI.Create(0, 0, 75, 100),
				DirtyRects: [SKRectI.Create(75, 0, 25, 100)]
			),
		];
		yield return
		[
			new MoveTestCase(
				"Right",
				Size: new SKSizeI(100, 100),
				Offset: new PositionOffset(25, 0),
				SourceRect: SKRectI.Create(0, 0, 75, 100),
				DestRect: SKRectI.Create(25, 0, 75, 100),
				DirtyRects: [SKRectI.Create(0, 0, 25, 100)]
			),
		];
		yield return
		[
			new MoveTestCase(
				"Up",
				Size: new SKSizeI(100, 100),
				Offset: new PositionOffset(0, -25),
				SourceRect: SKRectI.Create(0, 25, 100, 75),
				DestRect: SKRectI.Create(0, 0, 100, 75),
				DirtyRects: [SKRectI.Create(0, 75, 100, 25)]
			),
		];
		yield return
		[
			new MoveTestCase(
				"Down",
				Size: new SKSizeI(100, 100),
				Offset: new PositionOffset(0, 25),
				SourceRect: SKRectI.Create(0, 0, 100, 75),
				DestRect: SKRectI.Create(0, 25, 100, 75),
				DirtyRects: [SKRectI.Create(0, 0, 100, 25)]
			),
		];
		yield return
		[
			new MoveTestCase(
				"Up Left",
				Size: new SKSizeI(100, 100),
				Offset: new PositionOffset(-25, -25),
				SourceRect: SKRectI.Create(25, 25, 75, 75),
				DestRect: SKRectI.Create(0, 0, 75, 75),
				DirtyRects: [SKRectI.Create(75, 0, 25, 100), SKRectI.Create(0, 75, 75, 25)]
			),
		];
		yield return
		[
			new MoveTestCase(
				"Up Right",
				Size: new SKSizeI(100, 100),
				Offset: new PositionOffset(25, -25),
				SourceRect: SKRectI.Create(0, 25, 75, 75),
				DestRect: SKRectI.Create(25, 0, 75, 75),
				DirtyRects: [SKRectI.Create(0, 0, 25, 100), SKRectI.Create(25, 75, 75, 25)]
			),
		];
		yield return
		[
			new MoveTestCase(
				"Down Left",
				Size: new SKSizeI(100, 100),
				Offset: new PositionOffset(-25, 25),
				SourceRect: SKRectI.Create(25, 0, 75, 75),
				DestRect: SKRectI.Create(0, 25, 75, 75),
				DirtyRects: [SKRectI.Create(75, 0, 25, 100), SKRectI.Create(0, 0, 75, 25)]
			),
		];
		yield return
		[
			new MoveTestCase(
				"Down Right",
				Size: new SKSizeI(100, 100),
				Offset: new PositionOffset(25, 25),
				SourceRect: SKRectI.Create(0, 0, 75, 75),
				DestRect: SKRectI.Create(25, 25, 75, 75),
				DirtyRects: [SKRectI.Create(0, 0, 25, 100), SKRectI.Create(25, 0, 75, 25)]
			),
		];
	}

	[Theory]
	[MemberData(nameof(MoveTestCases))]
	public void ShouldComputeInstructionsForMoving(MoveTestCase testCase)
	{
		var inst = RenderInstructions.Everything(testCase.Size).Move(testCase.Offset);

		inst.PasteFrontBuffer.ShouldBeTrue();
		inst.SourceRect.ShouldBe(testCase.SourceRect);
		inst.DestRect.ShouldBe(testCase.DestRect);

		inst.GetDirtyRectangles().ShouldBe(testCase.DirtyRects, ignoreOrder: true);
	}
}
