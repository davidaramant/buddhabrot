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

        inst.PasteFrontBuffer.Should().BeFalse();

        var rects = inst.GetDirtyRectangles().ToList();
        rects.Should().HaveCount(1);
        rects.Should().Contain(new Rectangle(0, 0, 100, 100));
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
        yield return new object[]
        {
            new ResizeTestCase(
                "Smaller",
                OldSize: new PixelSize(100, 100),
                NewSize: new PixelSize(50, 50),
                DirtyRects: Enumerable.Empty<Rectangle>())
        };
        yield return new object[]
        {
            new ResizeTestCase(
                "Wider",
                OldSize: new PixelSize(100, 100),
                NewSize: new PixelSize(150, 100),
                DirtyRects: new[]
                {
                    new Rectangle(100, 0, 50, 100)
                })
        };
        yield return new object[]
        {
            new ResizeTestCase(
                "Taller",
                OldSize: new PixelSize(100, 100),
                NewSize: new PixelSize(100, 150),
                DirtyRects: new[]
                {
                    new Rectangle(0, 100, 100, 50)
                })
        };
        yield return new object[]
        {
            new ResizeTestCase(
                "Bigger",
                OldSize: new PixelSize(100, 100),
                NewSize: new PixelSize(150, 150),
                DirtyRects: new[]
                {
                    new Rectangle(100, 0, 50, 150),
                    new Rectangle(0, 100, 100, 50)
                })
        };
    }

    [Theory]
    [MemberData(nameof(ResizeTestCases))]
    public void ShouldComputeInstructionsForResizing(ResizeTestCase testCase)
    {
        var inst = RenderInstructions.Resized(testCase.OldSize, testCase.NewSize);

        inst.PasteFrontBuffer.Should().BeTrue();
        
        inst.GetDirtyRectangles().Should().BeEquivalentTo(testCase.DirtyRects);
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
        yield return new object[]
        {
            new MoveTestCase(
                "Left",
                PixelSize: new PixelSize(100, 100),
                Offset:new PixelVector(-25,0),
                SourceRect: new Rect(25,0,75,100),
                DestRect: new Rect(0,0,75,100),
                DirtyRects: new[]
                {
                    new Rectangle(75, 0, 25, 100)
                })
        };
        yield return new object[]
        {
            new MoveTestCase(
                "Right",
                PixelSize: new PixelSize(100, 100),
                Offset:new PixelVector(25,0),
                SourceRect: new Rect(0,0,75,100),
                DestRect: new Rect(25,0,75,100),
                DirtyRects: new[]
                {
                    new Rectangle(0, 0, 25, 100)
                })
        };
        yield return new object[]
        {
            new MoveTestCase(
                "Up",
                PixelSize: new PixelSize(100, 100),
                Offset:new PixelVector(0,-25),
                SourceRect: new Rect(0,25,100,75),
                DestRect: new Rect(0,0,100,75),
                DirtyRects: new[]
                {
                    new Rectangle(0, 75, 100, 25)
                })
        };
        yield return new object[]
        {
            new MoveTestCase(
                "Down",
                PixelSize: new PixelSize(100, 100),
                Offset:new PixelVector(0,25),
                SourceRect: new Rect(0,0,100,75),
                DestRect: new Rect(0,25,100,75),
                DirtyRects: new[]
                {
                    new Rectangle(0, 0, 100, 25)
                })
        };
        yield return new object[]
        {
            new MoveTestCase(
                "Up Left",
                PixelSize: new PixelSize(100, 100),
                Offset:new PixelVector(-25,-25),
                SourceRect: new Rect(25,25,75,75),
                DestRect: new Rect(0,0,75,75),
                DirtyRects: new[]
                {
                    new Rectangle(75, 0, 25, 100),
                    new Rectangle(0, 75, 75, 25),
                })
        };
        yield return new object[]
        {
            new MoveTestCase(
                "Up Right",
                PixelSize: new PixelSize(100, 100),
                Offset:new PixelVector(25,-25),
                SourceRect: new Rect(0,25,75,75),
                DestRect: new Rect(25,0,75,75),
                DirtyRects: new[]
                {
                    new Rectangle(0, 0, 25, 100),
                    new Rectangle(25, 75, 75, 25),
                })
        };
        yield return new object[]
        {
            new MoveTestCase(
                "Down Left",
                PixelSize: new PixelSize(100, 100),
                Offset:new PixelVector(-25,25),
                SourceRect: new Rect(25,0,75,75),
                DestRect: new Rect(0,25,75,75),
                DirtyRects: new[]
                {
                    new Rectangle(75, 0, 25, 100),
                    new Rectangle(0, 0, 75, 25),
                })
        };
        yield return new object[]
        {
            new MoveTestCase(
                "Down Right",
                PixelSize: new PixelSize(100, 100),
                Offset:new PixelVector(25,25),
                SourceRect: new Rect(0,0,75,75),
                DestRect: new Rect(25,25,75,75),
                DirtyRects: new[]
                {
                    new Rectangle(0, 0, 25, 100),
                    new Rectangle(25, 0, 75, 25),
                })
        };
    }

    [Theory]
    [MemberData(nameof(MoveTestCases))]
    public void ShouldComputeInstructionsForMoving(MoveTestCase testCase)
    {
        var inst = RenderInstructions.Moved(testCase.PixelSize, testCase.Offset);

        inst.PasteFrontBuffer.Should().BeTrue();
        inst.SourceRect.Should().Be(testCase.SourceRect);        
        inst.DestRect.Should().Be(testCase.DestRect);        

        inst.GetDirtyRectangles().Should().BeEquivalentTo(testCase.DirtyRects);
    }
}