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
        ParamsChanged,
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
        if (@event == Event.ParamsChanged)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (Lookup == null)
            {
                RecreateBuffers();
            }
            else
            {
                RecreateBuffers();
                await StartRenderingAsync();
                return RenderState.RenderingNormal;
            }
        }

        return _state;
    }

    private async Task<RenderState> HandleEventInRenderingNormalAsync(Event @event)
    {
        switch (@event)
        {
            case Event.ParamsChanged:
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
            await StartRenderingAsync();
            return RenderState.RenderingNormal;
        }

        return RenderState.RenderingPaused;
    }

    private async Task<RenderState> HandleEventInIdleAsync(Event @event)
    {
        if (@event == Event.ParamsChanged)
        {
            await StartRenderingAsync();
            return RenderState.RenderingNormal;
        }
        else if (@event == Event.StartPan)
        {
            return RenderState.Panning;
        }

        return RenderState.Idle;
    }

    private async Task<RenderState> HandleEventInPanningAsync(Event @event)
    {
        if (@event == Event.EndPan)
        {
            await StartRenderingAsync();
            return RenderState.RenderingNormal;
        }

        return RenderState.Panning;
    }

    private void RecreateBuffers()
    {
        var width = (int)Bounds.Width;
        var height = (int)Bounds.Height;

        if (_frontBuffer.PixelSize.Width != width || _frontBuffer.PixelSize.Height != height)
        {
            _frontBuffer = new RenderTargetBitmap(new PixelSize(width, height), new Vector(96, 96));
            _backBuffer = new RenderTargetBitmap(new PixelSize(width, height), new Vector(96, 96));
        }
    }

    private async Task StartRenderingAsync()
    {
        var args = await Dispatcher.UIThread.InvokeAsync(() => new RenderingArgs(SetBoundary, Lookup));

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
                await HandleEventAsync(Event.ParamsChanged);
            }
            else if (e.Property.Name == nameof(SetBoundary))
            {
                await HandleEventAsync(Event.ParamsChanged);
            }
        };

        this.EffectiveViewportChanged += async (_, _) => { await HandleEventAsync(Event.ParamsChanged); };

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
        SquareBoundary SetBoundary,
        RegionLookup Lookup);

    private Task RenderBuffersAsync(RenderingArgs args, CancellationToken cancelToken)
    {
        // TODO: Check for cancellation
        using (var context = _backBuffer.CreateDrawingContext(null))
        {
            var skiaContext = (ISkiaDrawingContextImpl)context;
            var canvas = skiaContext.SkCanvas;

            canvas.DrawRect(0, 0, _backBuffer.PixelSize.Width, _backBuffer.PixelSize.Height,
                new SKPaint { Color = SKColors.LightGray });

            var center = args.SetBoundary.Center;
            var radius = args.SetBoundary.QuadrantLength;

            canvas.DrawCircle(center.X, center.Y, radius, new SKPaint { Color = SKColors.White });

            var areasToDraw =
                args.Lookup.GetVisibleAreas(args.SetBoundary,
                    new Rectangle(0, 0, (int)Bounds.Width, (int)Bounds.Height));
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

        (_backBuffer, _frontBuffer) = (_frontBuffer, _backBuffer);

        return HandleEventAsync(Event.DoneRendering);
    }

    private void ResetLogicalArea() =>
        SetBoundary = SquareBoundary.GetLargestCenteredSquareInside((int)Bounds.Width, (int)Bounds.Height);

    protected override Avalonia.Size MeasureOverride(Avalonia.Size availableSize) => availableSize;
}