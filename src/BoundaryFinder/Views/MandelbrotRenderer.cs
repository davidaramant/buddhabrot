using System.Drawing;
using System.Net.Mime;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Media.Immutable;
using Avalonia.Skia;
using Buddhabrot.Core;
using Buddhabrot.Core.Boundary;
using ReactiveUI;
using SkiaSharp;
using Brushes = Avalonia.Media.Brushes;
using Point = Avalonia.Point;

namespace BoundaryFinder.Views;

public sealed class MandelbrotRenderer : Control
{
    private bool _panning;
    private Point _panningStartPoint;
    private SquareBoundary _panningStart;

    private enum RenderState
    {
        Uninitialized,
        Rendering,
        Done,
    }

    private volatile RenderState _renderState = RenderState.Uninitialized;
    private RenderTargetBitmap _frontBuffer = new RenderTargetBitmap(new PixelSize(1, 1));
    private RenderTargetBitmap _backBuffer = new RenderTargetBitmap(new PixelSize(1, 1));

    public static readonly StyledProperty<SquareBoundary> SetBoundaryProperty =
        AvaloniaProperty.Register<MandelbrotRenderer, SquareBoundary>(nameof(SetBoundary));

    public SquareBoundary SetBoundary
    {
        get => GetValue(SetBoundaryProperty);
        set => SetValue(SetBoundaryProperty, value);
    }

    public static readonly StyledProperty<ViewPort> ViewPortProperty =
        AvaloniaProperty.Register<MandelbrotRenderer, ViewPort>(nameof(ViewPort));

    public ViewPort ViewPort
    {
        get => GetValue(ViewPortProperty);
        set => SetValue(ViewPortProperty, value);
    }

    public static readonly StyledProperty<RegionLookup> LookupProperty =
        AvaloniaProperty.Register<MandelbrotRenderer, RegionLookup>(nameof(Lookup));

    public RegionLookup Lookup
    {
        get => GetValue(LookupProperty);
        set => SetValue(LookupProperty, value);
    }

    public ReactiveCommand<Unit, Unit> ResetViewCommand { get; }
    public ReactiveCommand<Unit, Unit> ZoomOutCommand { get; }

    static MandelbrotRenderer()
    {
        AffectsRender<MandelbrotRenderer>(LookupProperty);
        AffectsRender<MandelbrotRenderer>(SetBoundaryProperty);
    }

    public MandelbrotRenderer()
    {
        ClipToBounds = true;
        // HACK: I'm sure there is some fancy Reactive way to do this
        this.PropertyChanged += (s, e) =>
        {
            if (e.Property.Name == nameof(Lookup) && Lookup?.NodeCount > 1)
            {
                ResetLogicalArea();
            }
        };

        ResetViewCommand = ReactiveCommand.Create(ResetLogicalArea);
        ZoomOutCommand = ReactiveCommand.Create(() =>
        {
            SetBoundary = SetBoundary.ZoomOut((int)Bounds.Width, (int)Bounds.Height);
        });
    }

    private void InitializeBitmaps(int width, int height)
    {
        if (_frontBuffer.PixelSize.Width != width || _frontBuffer.PixelSize.Height != height)
        {
            // TODO: Handle resizing
            _frontBuffer = new RenderTargetBitmap(new PixelSize(width, height), new Vector(96, 96));
            _backBuffer = new RenderTargetBitmap(new PixelSize(width, height), new Vector(96, 96));
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (e.ClickCount == 1)
        {
            _panning = true;
            _panningStartPoint = e.GetPosition(this);
            _panningStart = SetBoundary;
        }
        else if (e.ClickCount == 2)
        {
            _panning = false;
            var pos = e.GetPosition(this);
            SetBoundary = SetBoundary.ZoomIn((int)pos.X, (int)pos.Y);
        }

        base.OnPointerPressed(e);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        _panning = false;
        base.OnPointerReleased(e);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        if (_panning)
        {
            var currentPos = e.GetPosition(this);
            var deltaX = (int)(currentPos.X - _panningStartPoint.X);
            var deltaY = (int)(currentPos.Y - _panningStartPoint.Y);

            SetBoundary = _panningStart.OffsetBy(deltaX, deltaY);
        }

        base.OnPointerMoved(e);
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        _panning = false;
        base.OnPointerCaptureLost(e);
    }

    public override void Render(DrawingContext context)
    {
        var size = new System.Drawing.Size((int)Bounds.Width, (int)Bounds.Height);

        ViewPort = ViewPort.FromResolution(
            size,
            SetBoundary.Center,
            2d / SetBoundary.QuadrantLength);

        InitializeBitmaps(size.Width, size.Height);

        if (_renderState == RenderState.Uninitialized)
        {
            _renderState = RenderState.Rendering;
            ThreadPool.QueueUserWorkItem(RenderBuffers);
        }

        if (_renderState == RenderState.Done)
        {
            context.FillRectangle(Brushes.Orange, new Rect(Bounds.Size));
            // context.DrawImage(_frontBuffer,
            //     new Rect(0, 0, _frontBuffer.PixelSize.Width, _frontBuffer.PixelSize.Height),
            //     new Rect(0, 0, Width, Height)
            // )
            // TODO: Why doesn't this work???????????
            context.DrawImage(_frontBuffer,
                new Rect(0, 0, 100, 100),
                new Rect(0, 0, 100, 100)
            );
        }

        // context.FillRectangle(Brushes.LightGray, new Rect(Bounds.Size));
        //
        // var center = SetBoundary.Center;
        // var radius = SetBoundary.QuadrantLength;
        // context.DrawEllipse(Brushes.White, null, new Point(center.X, center.Y), radius, radius);
        //
        // var areasToDraw =
        //     Lookup.GetVisibleAreas(SetBoundary, new Rectangle(0, 0, (int)Bounds.Width, (int)Bounds.Height));
        // for (var index = 0; index < areasToDraw.Count; index++)
        // {
        //     var (area, type) = areasToDraw[index];
        //     var brush = type switch
        //     {
        //         RegionType.Border => Brushes.DarkSlateBlue,
        //         RegionType.Filament => Brushes.Red,
        //         _ => Brushes.White,
        //     };
        //     context.FillRectangle(brush, new Rect(area.X, area.Y, area.Width, area.Height));
        // }
    }

    private void RenderBuffers(object? _)
    {
        // TODO: This method can't access any UI properties. It needs to be passed in all the dependencies

        using (var context = _backBuffer.CreateDrawingContext(null))
        {
            context.Clear(Colors.Aqua);

            //var skiaContext = (ISkiaDrawingContextImpl)context;
            //var canvas = skiaContext.SkCanvas;

            //canvas.DrawRect(0, 0, _backBuffer.PixelSize.Width, _backBuffer.PixelSize.Height, new SKPaint { Color = SKColors.Red });


            // var center = SetBoundary.Center;
            // var radius = SetBoundary.QuadrantLength;
            //
            // canvas.DrawCircle(center.X, center.Y, radius, new SKPaint { Color = SKColors.White });
            //
            // var areasToDraw =
            //     Lookup.GetVisibleAreas(SetBoundary, new Rectangle(0, 0, (int)Bounds.Width, (int)Bounds.Height));
            // for (var index = 0; index < areasToDraw.Count; index++)
            // {
            //     var (area, type) = areasToDraw[index];
            //     var color = type switch
            //     {
            //         RegionType.Border => SKColors.DarkSlateBlue,
            //         RegionType.Filament => SKColors.Red,
            //         _ => SKColors.White,
            //     };
            //
            //     canvas.DrawRect(area.X, area.Y, area.Width, area.Height, new SKPaint { Color = color });
            // }
        }

        (_backBuffer, _frontBuffer) = (_frontBuffer, _backBuffer);

        _renderState = RenderState.Done;
    }

    private void ResetLogicalArea() =>
        SetBoundary = SquareBoundary.GetLargestCenteredSquareInside((int)Bounds.Width, (int)Bounds.Height);

    protected override Avalonia.Size MeasureOverride(Avalonia.Size availableSize) => availableSize;
}