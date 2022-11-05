using System;
using System.Collections.Generic;
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

namespace BoundaryFinder.Views;

// TODO: Move the creation of _nextInstructions out of the machine, combined NewData, Resize, Zoom, and Panned events
// TODO: IsBusy indicator (why did it break EVERYTHING????)

public sealed class MandelbrotRenderer : Control
{
    private bool _isPanning;
    private Point _panningStartPoint;
    private SquareBoundary _panningStart;
    private PixelVector _panningOffset = new();
    private RenderInstructions _nextInstructions = RenderInstructions.Everything(new PixelSize(1, 1));

    private enum RenderState
    {
        Uninitialized,
        Rendering,
        Idle,
    }

    private enum Event
    {
        NewData,
        Resize,
        Zoom,
        Panned,
        DoneRendering,
    }

    private readonly Queue<Event> _eventQueue = new();
    private bool _handlingQueue = false;
    private RenderState _state = RenderState.Uninitialized;

    private PixelSize PixelBounds => new(Math.Max(1, (int)Bounds.Width), Math.Max(1, (int)Bounds.Height));


    private async Task HandleEventAsync(Event @event)
    {
        Console.Out.WriteLine($"{_state}: {@event}");
        _eventQueue.Enqueue(@event);

        if (_handlingQueue)
        {
            Console.Out.WriteLine("bailing, event queued...");
            return;
        }

        _handlingQueue = true;
        while (_eventQueue.TryDequeue(out var e))
        {
            _state = await (_state switch
            {
                RenderState.Uninitialized => HandleEventInUninitializedAsync(e),
                RenderState.Rendering => HandleEventInRenderingAsync(e),
                RenderState.Idle => HandleEventInIdleAsync(e),
                _ => throw new ArgumentOutOfRangeException()
            });
        }

        _handlingQueue = false;
    }

    private async Task<RenderState> HandleEventInUninitializedAsync(Event @event)
    {
        if (@event == Event.NewData)
        {
            _nextInstructions = RenderInstructions.Everything(PixelBounds);
            await StartRenderingAsync();
            return RenderState.Rendering;
        }

        return _state;
    }

    private async Task<RenderState> HandleEventInRenderingAsync(Event @event)
    {
        switch (@event)
        {
            case Event.NewData:
            case Event.Zoom:
                _nextInstructions = RenderInstructions.Everything(PixelBounds);
                await CancelRenderingAsync();
                await StartRenderingAsync();
                return RenderState.Rendering;

            case Event.Resize:
                _nextInstructions = RenderInstructions.Resized(oldSize: _frontBuffer.PixelSize, newSize: PixelBounds);
                await CancelRenderingAsync();
                await StartRenderingAsync();
                return RenderState.Rendering;

            case Event.Panned:
                _nextInstructions = RenderInstructions.Moved(PixelBounds, _panningOffset);
                SetBoundary = _panningStart.OffsetBy(_panningOffset.X, _panningOffset.Y);
                await CancelRenderingAsync();
                await StartRenderingAsync();
                return RenderState.Rendering;

            case Event.DoneRendering:
                InvalidateVisual();
                return RenderState.Idle;

            default:
                return RenderState.Rendering;
        }
    }

    private async Task<RenderState> HandleEventInIdleAsync(Event @event)
    {
        switch (@event)
        {
            case Event.NewData:
            case Event.Zoom:
                _nextInstructions = RenderInstructions.Everything(PixelBounds);
                await StartRenderingAsync();
                return RenderState.Rendering;

            case Event.Resize:
                _nextInstructions = RenderInstructions.Resized(oldSize: _frontBuffer.PixelSize, newSize: PixelBounds);
                await StartRenderingAsync();
                return RenderState.Rendering;

            case Event.Panned:
                _nextInstructions = RenderInstructions.Moved(PixelBounds, _panningOffset);
                SetBoundary = _panningStart.OffsetBy(_panningOffset.X, _panningOffset.Y);
                await StartRenderingAsync();
                return RenderState.Rendering;

            default:
                return RenderState.Idle;
        }
    }

    private async Task StartRenderingAsync()
    {
        var args = await Dispatcher.UIThread.InvokeAsync(
            () => new RenderingArgs(_nextInstructions, SetBoundary, Lookup));

        _renderingTask = Task.Run(() => RenderBuffersAsync(args, _cancelSource.Token));
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
        PointerPressed += async (_, e) =>
        {
            if (e.ClickCount == 1)
            {
                _isPanning = true;
                _panningStartPoint = e.GetPosition(this);
                _panningStart = SetBoundary;
            }
            else if (e.ClickCount == 2)
            {
                _isPanning = false;
                var pos = e.GetPosition(this);
                SetBoundary = SetBoundary.ZoomIn((int)pos.X, (int)pos.Y);
                await HandleEventAsync(Event.Zoom);
            }
        };
        PointerReleased += async (_, e) =>
        {
            _isPanning = false;
            await HandleEventAsync(Event.Panned);
        };
        PointerCaptureLost += async (_, e) =>
        {
            if (_isPanning)
            {
                _isPanning = true;
                await HandleEventAsync(Event.Panned);
            }
        };

        ResetViewCommand = ReactiveCommand.Create(ResetLogicalArea);
        ZoomOutCommand = ReactiveCommand.Create(() =>
        {
            SetBoundary = SetBoundary.ZoomOut((int)Bounds.Width, (int)Bounds.Height);
        });
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        if (_isPanning)
        {
            var currentPos = e.GetPosition(this);
            var deltaX = (int)(currentPos.X - _panningStartPoint.X);
            var deltaY = (int)(currentPos.Y - _panningStartPoint.Y);

            _panningOffset = new PixelVector(deltaX, deltaY);
            InvalidateVisual();
        }

        base.OnPointerMoved(e);
    }

    public override void Render(DrawingContext context)
    {
        var size = new System.Drawing.Size((int)Bounds.Width, (int)Bounds.Height);

        ViewPort = ViewPort.FromResolution(
            size,
            SetBoundary.Center,
            2d / SetBoundary.QuadrantLength);

        context.DrawImage(_frontBuffer,
            new Rect(
                _panningOffset.X,
                _panningOffset.Y,
                _frontBuffer.PixelSize.Width,
                _frontBuffer.PixelSize.Height)
        );
    }

    sealed record RenderingArgs(
        RenderInstructions Instructions,
        SquareBoundary SetBoundary,
        RegionLookup Lookup)
    {
        public int Width => Instructions.Size.Width;
        public int Height => Instructions.Size.Height;
    }

    private Task RenderBuffersAsync(RenderingArgs args, CancellationToken cancelToken)
    {
        if (_backBuffer.PixelSize.Width != args.Width || _backBuffer.PixelSize.Height != args.Height)
        {
            _backBuffer = new RenderTargetBitmap(new PixelSize(args.Width, args.Height), new Vector(96, 96));
        }

        // TODO: Check for cancellation
        using (var context = _backBuffer.CreateDrawingContext(null))
        {
            var skiaContext = (ISkiaDrawingContextImpl)context;
            var canvas = skiaContext.SkCanvas;

            canvas.DrawRect(0, 0, args.Width, args.Height,
                new SKPaint { Color = SKColors.LightGray });

            var center = args.SetBoundary.Center;
            var radius = args.SetBoundary.QuadrantLength;

            canvas.DrawCircle(center.X, center.Y, radius, new SKPaint { Color = SKColors.White });

            if (args.Instructions.PasteFrontBuffer)
            {
                context.DrawBitmap(
                    _frontBuffer.PlatformImpl,
                    opacity: 1,
                    sourceRect: args.Instructions.SourceRect,
                    destRect: args.Instructions.DestRect);
            }

            foreach (var dirtyRect in args.Instructions.GetDirtyRectangles())
            {
                Console.Out.WriteLine(dirtyRect);
                var areasToDraw =
                    args.Lookup.GetVisibleAreas(args.SetBoundary, dirtyRect);
                foreach (var (area, type) in areasToDraw)
                {
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
        _panningOffset = new();

        return HandleEventAsync(Event.DoneRendering);
    }

    private void ResetLogicalArea() =>
        SetBoundary = SquareBoundary.GetLargestCenteredSquareInside((int)Bounds.Width, (int)Bounds.Height);

    protected override Size MeasureOverride(Size availableSize) => availableSize;
}