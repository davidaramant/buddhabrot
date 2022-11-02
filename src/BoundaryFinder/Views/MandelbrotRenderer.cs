using System;
using System.Drawing;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Skia;
using Avalonia.Threading;
using Buddhabrot.Core;
using Buddhabrot.Core.Boundary;
using ReactiveUI;
using SkiaSharp;
using Point = Avalonia.Point;

namespace BoundaryFinder.Views;

public sealed class MandelbrotRenderer : Control
{
    private bool _panning;
    private Point _panningStartPoint;
    private SquareBoundary _panningStart;
    private RenderInstructions _nextInstructions = RenderInstructions.Everything(new Avalonia.Size(1, 1));

    private enum RenderState
    {
        Uninitialized,
        RenderingNormal,
        RenderingPaused,
        Idle,
        Panning,
    }

    private enum Event
    {
        NewData,
        Resize,
        Zoom,
        DoneRendering,
        StartPan,
        EndPan,
    }

    private RenderState _state = RenderState.Uninitialized;

    private async Task HandleEventAsync(Event @event) =>
        _state = await (_state switch
        {
            RenderState.Uninitialized => HandleEventInUninitializedAsync(@event),
            RenderState.RenderingNormal => HandleEventInRenderingNormalAsync(@event),
            RenderState.RenderingPaused => HandleEventInRenderingPausedAsync(@event),
            RenderState.Idle => HandleEventInIdleAsync(@event),
            RenderState.Panning => HandleEventInPanningAsync(@event),
            _ => throw new ArgumentOutOfRangeException()
        });

    private async Task<RenderState> HandleEventInUninitializedAsync(Event @event)
    {
        if (@event == Event.NewData)
        {
            _nextInstructions = RenderInstructions.Everything(Bounds.Size);
            await StartRenderingAsync();
            return RenderState.RenderingNormal;
        }

        return _state;
    }

    private async Task<RenderState> HandleEventInRenderingNormalAsync(Event @event)
    {
        switch (@event)
        {
            case Event.NewData:
            case Event.Zoom:
                _nextInstructions = RenderInstructions.Everything(Bounds.Size);
                await CancelRenderingAsync();
                await StartRenderingAsync();
                return RenderState.RenderingNormal;

            case Event.Resize:
                _nextInstructions = RenderInstructions.Resized(oldSize: _frontBuffer.Size, newSize: Bounds.Size);
                await CancelRenderingAsync();
                await StartRenderingAsync();
                return RenderState.RenderingNormal;

            case Event.DoneRendering:
                InvalidateVisual();
                return RenderState.Idle;

            case Event.StartPan:
                await CancelRenderingAsync();
                return RenderState.RenderingPaused;

            default:
                return RenderState.RenderingNormal;
        }
    }

    private async Task<RenderState> HandleEventInRenderingPausedAsync(Event @event)
    {
        if (@event == Event.EndPan)
        {
            _nextInstructions = RenderInstructions.Moved(Bounds.Size, new Point(0, 0)); // TODO: Pass in panning offset
            await StartRenderingAsync();
            return RenderState.RenderingNormal;
        }

        return RenderState.RenderingPaused;
    }

    private async Task<RenderState> HandleEventInIdleAsync(Event @event)
    {
        switch (@event)
        {
            case Event.NewData:
            case Event.Zoom:
                _nextInstructions = RenderInstructions.Everything(Bounds.Size);
                await StartRenderingAsync();
                return RenderState.RenderingNormal;

            case Event.Resize:
                _nextInstructions = RenderInstructions.Resized(oldSize: _frontBuffer.Size, newSize: Bounds.Size);
                await StartRenderingAsync();
                return RenderState.RenderingNormal;

            case Event.StartPan:
                return RenderState.Panning;
            default:
                return RenderState.Idle;
        }
    }

    private async Task<RenderState> HandleEventInPanningAsync(Event @event)
    {
        if (@event == Event.EndPan)
        {
            _nextInstructions = RenderInstructions.Moved(Bounds.Size, new Point(0, 0)); // TODO: Pass in panning offset
            await StartRenderingAsync();
            return RenderState.RenderingNormal;
        }

        return RenderState.Panning;
    }

    private async Task StartRenderingAsync()
    {
        var args = await Dispatcher.UIThread.InvokeAsync(
            () => new RenderingArgs(_nextInstructions, SetBoundary, Lookup));

        _renderingTask =
            Task.Run(() => RenderBuffersAsync(args, _cancelSource.Token));
    }

    private async Task CancelRenderingAsync()
    {
        _cancelSource.Cancel();
        await _renderingTask;
        _cancelSource = new();
    }

    private CancellationTokenSource _cancelSource = new();
    private Task _renderingTask = Task.CompletedTask;


    private RenderTargetBitmap _frontBuffer = new(new PixelSize(1, 1));
    private RenderTargetBitmap _backBuffer = new(new PixelSize(1, 1));

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
        this.PropertyChanged += async (s, e) =>
        {
            if (e.Property.Name == nameof(Lookup) && Lookup?.NodeCount > 1)
            {
                ResetLogicalArea();
                await HandleEventAsync(Event.NewData);
            }
            else if (e.Property.Name == nameof(SetBoundary))
            {
                await HandleEventAsync(Event.Zoom);
            }
        };

        this.EffectiveViewportChanged += async (_, _) => { await HandleEventAsync(Event.Resize); };

        ResetViewCommand = ReactiveCommand.Create(ResetLogicalArea);
        ZoomOutCommand = ReactiveCommand.Create(() =>
        {
            SetBoundary = SetBoundary.ZoomOut((int)Bounds.Width, (int)Bounds.Height);
        });
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

        context.DrawImage(_frontBuffer,
            new Rect(0, 0, _frontBuffer.PixelSize.Width, _frontBuffer.PixelSize.Height)
        );
    }

    sealed record RenderingArgs(
        RenderInstructions Instructions,
        SquareBoundary SetBoundary,
        RegionLookup Lookup);

    private Task RenderBuffersAsync(RenderingArgs args, CancellationToken cancelToken)
    {
        var width = (int)Bounds.Width;
        var height = (int)Bounds.Height;

        if (_backBuffer.PixelSize.Width != width || _backBuffer.PixelSize.Height != height)
        {
            _backBuffer = new RenderTargetBitmap(new PixelSize(width, height), new Vector(96, 96));
        }

        // TODO: Check for cancellation
        using (var context = _backBuffer.CreateDrawingContext(null))
        {
            var skiaContext = (ISkiaDrawingContextImpl)context;
            var canvas = skiaContext.SkCanvas;

            canvas.DrawRect(0, 0, width, height,
                new SKPaint { Color = SKColors.LightGray });

            var center = args.SetBoundary.Center;
            var radius = args.SetBoundary.QuadrantLength;

            canvas.DrawCircle(center.X, center.Y, radius, new SKPaint { Color = SKColors.White });

            if (args.Instructions.PasteFrontBuffer)
            {
                context.DrawBitmap(
                    _frontBuffer.PlatformImpl,
                    opacity: 1,
                    sourceRect: new Rect(new Point(0, 0), _frontBuffer.Size),
                    destRect: new Rect(args.Instructions.PasteOffset.Value.X, args.Instructions.PasteOffset.Value.Y,
                        width, height));
            }

            foreach (var dirtyRect in args.Instructions.GetDirtyRectangles())
            {
                var areasToDraw =
                    args.Lookup.GetVisibleAreas(args.SetBoundary,
                        new System.Drawing.Rectangle((int)dirtyRect.X, (int)dirtyRect.Y, (int)dirtyRect.Width,
                            (int)dirtyRect.Height));
                for (var index = 0; index < areasToDraw.Count; index++)
                {
                    var (area, type) = areasToDraw[index];
                    var color = type switch
                    {
                        RegionType.Border => SKColors.DarkSlateBlue,
                        RegionType.Filament => SKColors.Red,
                        _ => SKColors.White,
                    };

                    canvas.DrawRect(area.X, area.Y, area.Width, area.Height, new SKPaint { Color = color });
                }
            }
        }

        (_backBuffer, _frontBuffer) = (_frontBuffer, _backBuffer);

        return HandleEventAsync(Event.DoneRendering);
    }

    private void ResetLogicalArea() =>
        SetBoundary = SquareBoundary.GetLargestCenteredSquareInside((int)Bounds.Width, (int)Bounds.Height);

    protected override Avalonia.Size MeasureOverride(Avalonia.Size availableSize) => availableSize;
}