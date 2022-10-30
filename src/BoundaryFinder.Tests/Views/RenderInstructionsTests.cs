using Avalonia;
using BoundaryFinder.Views;

namespace BoundaryFinder.Tests.Views;

public class RenderInstructionsTests
{
    [Fact]
    public void ShouldIncludeEntireAreaIfInvalidatingEverything()
    {
        var inst = RenderInstructions.Everything(new Size(100, 100));

        inst.PasteFrontBuffer.Should().BeFalse();

        var rects = inst.GetDirtyRectangles().ToList();
        rects.Should().HaveCount(1);
        rects.Should().Contain(new Rect(0, 0, 100, 100));
    }

    public sealed record ResizeTestCase(
        string Description,
        Size OldSize,
        Size NewSize,
        IEnumerable<Rect> DirtyRects
    )
    {
        public override string ToString() => Description;
    }

    public static IEnumerable<object[]> ResizeTestCases()
    {
        yield return new object[]
        {
            new ResizeTestCase(
                "Smaller",
                OldSize: new Size(100, 100),
                NewSize: new Size(50, 50),
                DirtyRects: Enumerable.Empty<Rect>())
        };
        yield return new object[]
        {
            new ResizeTestCase(
                "Wider",
                OldSize: new Size(100, 100),
                NewSize: new Size(150, 100),
                DirtyRects: new[]
                {
                    new Rect(100, 0, 50, 100)
                })
        };
        yield return new object[]
        {
            new ResizeTestCase(
                "Taller",
                OldSize: new Size(100, 100),
                NewSize: new Size(100, 150),
                DirtyRects: new[]
                {
                    new Rect(0, 100, 100, 50)
                })
        };
        yield return new object[]
        {
            new ResizeTestCase(
                "Bigger",
                OldSize: new Size(100, 100),
                NewSize: new Size(150, 150),
                DirtyRects: new[]
                {
                    new Rect(100, 0, 50, 150),
                    new Rect(0, 100, 100, 50)
                })
        };
    }

    [Theory]
    [MemberData(nameof(ResizeTestCases))]
    public void ShouldComputeInstructionsForResizing(ResizeTestCase testCase)
    {
        var inst = RenderInstructions.Resized(testCase.OldSize, testCase.NewSize);

        inst.PasteFrontBuffer.Should().BeTrue();
        inst.PasteOffset.Should().Be(new Point(0, 0));

        inst.GetDirtyRectangles().Should().BeEquivalentTo(testCase.DirtyRects);
    }
    
    public sealed record MoveTestCase(
        string Description,
        Size Size,
        Point Offset,
        IEnumerable<Rect> DirtyRects
    )
    {
        public override string ToString() => Description;
    }

    public static IEnumerable<object[]> MoveTestCases()
    {
        yield return new object[]
        {
            new MoveTestCase(
                "Left",
                Size: new Size(100, 100),
                Offset:new Point(-25,0),
                DirtyRects: new[]
                {
                    new Rect(75, 0, 25, 100)
                })
        };
        yield return new object[]
        {
            new MoveTestCase(
                "Right",
                Size: new Size(100, 100),
                Offset:new Point(25,0),
                DirtyRects: new[]
                {
                    new Rect(0, 0, 25, 100)
                })
        };
        yield return new object[]
        {
            new MoveTestCase(
                "Up",
                Size: new Size(100, 100),
                Offset:new Point(0,-25),
                DirtyRects: new[]
                {
                    new Rect(0, 75, 100, 25)
                })
        };
        yield return new object[]
        {
            new MoveTestCase(
                "Down",
                Size: new Size(100, 100),
                Offset:new Point(0,25),
                DirtyRects: new[]
                {
                    new Rect(0, 0, 100, 25)
                })
        };
        yield return new object[]
        {
            new MoveTestCase(
                "Up Left",
                Size: new Size(100, 100),
                Offset:new Point(-25,-25),
                DirtyRects: new[]
                {
                    new Rect(75, 0, 25, 100),
                    new Rect(0, 75, 75, 25),
                })
        };
        yield return new object[]
        {
            new MoveTestCase(
                "Up Right",
                Size: new Size(100, 100),
                Offset:new Point(25,-25),
                DirtyRects: new[]
                {
                    new Rect(0, 0, 25, 100),
                    new Rect(25, 75, 75, 25),
                })
        };
        yield return new object[]
        {
            new MoveTestCase(
                "Down Left",
                Size: new Size(100, 100),
                Offset:new Point(-25,25),
                DirtyRects: new[]
                {
                    new Rect(75, 0, 25, 100),
                    new Rect(0, 0, 75, 25),
                })
        };
        yield return new object[]
        {
            new MoveTestCase(
                "Down Right",
                Size: new Size(100, 100),
                Offset:new Point(25,25),
                DirtyRects: new[]
                {
                    new Rect(0, 0, 25, 100),
                    new Rect(25, 0, 75, 25),
                })
        };
    }

    [Theory]
    [MemberData(nameof(MoveTestCases))]
    public void ShouldComputeInstructionsForMoving(MoveTestCase testCase)
    {
        var inst = RenderInstructions.Moved(testCase.Size, testCase.Offset);

        inst.PasteFrontBuffer.Should().BeTrue();
        inst.PasteOffset.Should().Be(testCase.Offset);

        inst.GetDirtyRectangles().Should().BeEquivalentTo(testCase.DirtyRects);
    }
}