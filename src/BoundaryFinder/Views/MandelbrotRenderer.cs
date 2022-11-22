using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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
using Buddhabrot.Core.Boundary.Visualization;
using Buddhabrot.Core.Calculations;
using Buddhabrot.Core.ExtensionMethods.Drawing;
using ReactiveUI;
using SkiaSharp;
using Vector = Avalonia.Vector;

namespace BoundaryFinder.Views;

public sealed class MandelbrotRenderer : Control
{
    private SquareBoundary _setBoundary;
    private bool _isPanning;
    private Point _panningStartPoint;
    private SquareBoundary _panningStart;
    private PixelVector _panningOffset = new();

    private enum RenderState
    {
        Rendering,
        Idle,
    }

    private RenderState _state = RenderState.Idle;
    private RenderingArgs? _currentFrameArgs;
    private RenderingArgs? _nextFrameArgs;
    private CancellationTokenSource _cancelSource = new();
    private Task _renderingTask = Task.CompletedTask;

    private RenderTargetBitmap _frontBuffer = new(new PixelSize(1, 1));
    private RenderTargetBitmap _backBuffer = new(new PixelSize(1, 1));

    private PixelSize PixelBounds => new(Math.Max(1, (int)Bounds.Width), Math.Max(1, (int)Bounds.Height));

    public static readonly StyledProperty<ViewPort> ViewPortProperty =
        AvaloniaProperty.Register<MandelbrotRenderer, ViewPort>(nameof(ViewPort));

    public ViewPort ViewPort
    {
        get => GetValue(ViewPortProperty);
        set => SetValue(ViewPortProperty, value);
    }

    public static readonly StyledProperty<IBoundaryPalette> PaletteProperty =
        AvaloniaProperty.Register<MandelbrotRenderer, IBoundaryPalette>(nameof(Palette),
            defaultValue: PastelPalette.Instance);

    public IBoundaryPalette Palette
    {
        get => GetValue(PaletteProperty);
        set => SetValue(PaletteProperty, value);
    }

    public static readonly StyledProperty<RegionLookup> LookupProperty =
        AvaloniaProperty.Register<MandelbrotRenderer, RegionLookup>(nameof(Lookup));

    public RegionLookup Lookup
    {
        get => GetValue(LookupProperty);
        set => SetValue(LookupProperty, value);
    }

    public static readonly StyledProperty<bool> IsBusyProperty =
        AvaloniaProperty.Register<MandelbrotRenderer, bool>(nameof(IsBusy));

    public bool IsBusy
    {
        get => GetValue(IsBusyProperty);
        set => SetValue(IsBusyProperty, value);
    }

    public static readonly StyledProperty<bool> RenderInteriorsProperty =
        AvaloniaProperty.Register<MandelbrotRenderer, bool>(nameof(RenderInteriors));

    public bool RenderInteriors
    {
        get => GetValue(RenderInteriorsProperty);
        set => SetValue(RenderInteriorsProperty, value);
    }

    public static readonly StyledProperty<int> MaximumIterationsProperty =
        AvaloniaProperty.Register<MandelbrotRenderer, int>(nameof(MaximumIterations));

    public int MaximumIterations
    {
        get => GetValue(MaximumIterationsProperty);
        set => SetValue(MaximumIterationsProperty, value);
    }

    public static readonly StyledProperty<int> MinimumIterationsProperty =
        AvaloniaProperty.Register<MandelbrotRenderer, int>(nameof(MinimumIterations));

    public int MinimumIterations
    {
        get => GetValue(MinimumIterationsProperty);
        set => SetValue(MinimumIterationsProperty, value);
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
                await ResetLogicalAreaAsync();
            }
            else if (e.Property.Name == nameof(RenderInteriors))
            {
                await RequestRenderAsync(RenderInstructions.Everything(PixelBounds));
            }
        };

        this.EffectiveViewportChanged += async (_, _) =>
        {
            await RequestRenderAsync(RenderInstructions.Resized(oldSize: _frontBuffer.PixelSize,
                newSize: PixelBounds));
        };
        PointerPressed += async (_, e) =>
        {
            var properties = e.GetCurrentPoint(this).Properties;

            if (properties.IsLeftButtonPressed)
            {
                if (e.ClickCount == 1)
                {
                    _isPanning = true;
                    _panningStartPoint = e.GetPosition(this);
                    _panningStart = _setBoundary;
                }
                else if (e.ClickCount == 2)
                {
                    _isPanning = false;
                    var pos = e.GetPosition(this);
                    _setBoundary = _setBoundary.ZoomIn((int)pos.X, (int)pos.Y);
                    await RequestRenderAsync(RenderInstructions.Everything(PixelBounds));
                }
            }
            else if (properties.IsRightButtonPressed && e.ClickCount == 2)
            {
                _setBoundary = _setBoundary.ZoomOut(PixelBounds.Width, PixelBounds.Height);
                await RequestRenderAsync(RenderInstructions.Everything(PixelBounds));
            }
        };
        PointerReleased += async (_, e) =>
        {
            if (_isPanning)
            {
                _isPanning = false;
                _setBoundary = _panningStart.OffsetBy(_panningOffset.X, _panningOffset.Y);
                await RequestRenderAsync(RenderInstructions.Moved(PixelBounds, _panningOffset));
            }
        };
        PointerCaptureLost += async (_, e) =>
        {
            if (_isPanning)
            {
                _isPanning = false;

                _setBoundary = _panningStart.OffsetBy(_panningOffset.X, _panningOffset.Y);
                await RequestRenderAsync(RenderInstructions.Moved(PixelBounds, _panningOffset));
            }
        };

        ResetViewCommand = ReactiveCommand.CreateFromTask(ResetLogicalAreaAsync);
        ZoomOutCommand = ReactiveCommand.CreateFromTask(() =>
        {
            _setBoundary = _setBoundary.ZoomOut(PixelBounds.Width, PixelBounds.Height);
            return RequestRenderAsync(RenderInstructions.Everything(PixelBounds));
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
            _setBoundary.Center,
            2d / _setBoundary.QuadrantLength);

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
        RegionLookup Lookup,
        IBoundaryPalette Palette,
        ViewPort ViewPort,
        bool RenderInteriors,
        int MinIterations,
        int MaxIterations)
    {
        public int Width => Instructions.Size.Width;
        public int Height => Instructions.Size.Height;
    }

    private Task RenderToBufferAsync(RenderingArgs args, CancellationToken cancelToken)
    {
        if (_backBuffer.PixelSize.Width != args.Width || _backBuffer.PixelSize.Height != args.Height)
        {
            _backBuffer.Dispose();
            _backBuffer = new RenderTargetBitmap(new PixelSize(args.Width, args.Height), new Vector(96, 96));
        }

        // TODO: Check for cancellation
        using (var context = _backBuffer.CreateDrawingContext(null))
        {
            var skiaContext = (ISkiaDrawingContextImpl)context;
            var canvas = skiaContext.SkCanvas;

            canvas.DrawRect(0, 0, args.Width, args.Height,
                new SKPaint { Color = args.Palette.Background });

            var center = args.SetBoundary.Center;
            var radius = args.SetBoundary.QuadrantLength;

            canvas.DrawCircle(center.X, center.Y, radius, new SKPaint { Color = args.Palette.InBounds });

            if (args.Instructions.PasteFrontBuffer)
            {
                context.DrawBitmap(
                    _frontBuffer.PlatformImpl,
                    opacity: 1,
                    sourceRect: args.Instructions.SourceRect,
                    destRect: args.Instructions.DestRect);
            }

            var areasToDraw =
                args.Lookup.GetVisibleAreas(args.SetBoundary, args.Instructions.GetDirtyRectangles());

            using var paint = new SKPaint();

            var positionsToRender = new List<System.Drawing.Point>();

            foreach (var (area, type) in areasToDraw)
            {
                if (type == RegionType.Border && args.RenderInteriors)
                {
                    positionsToRender.AddRange(area.GetAllPositions());
                }
                else
                {
                    paint.Color = type switch
                    {
                        RegionType.Border => args.Palette.Border,
                        RegionType.Filament => args.Palette.Filament,
                        RegionType.Rejected => args.Palette.InSet,
                        _ => args.Palette.InBounds,
                    };

                    canvas.DrawRect(area.X, area.Y, area.Width, area.Height, paint);
                }
            }

            if (positionsToRender.Any())
            {
                var points = positionsToRender.Select(args.ViewPort.GetComplex).ToArray();
                var escapeTimes = new EscapeTime[points.Length];

                // TODO: move this to function
                // TODO: use vectors internally
                // TODO: Why does this lock up the UI? It's already in a different Task, should this part be in a Task
                // as well?
                Parallel.For(0, points.Length, i =>
                {
                    escapeTimes[i] = ScalarKernel.FindEscapeTime(points[i], args.MaxIterations);
                });

                for (int i = 0; i < points.Length; i++)
                {
                    var time = escapeTimes[i];
                    if (time.IsInfinite)
                    {
                        paint.Color = args.Palette.BorderInSet;
                    }
                    else if (time.Iterations > args.MinIterations)
                    {
                        paint.Color = args.Palette.BorderInRange;
                    }
                    else
                    {
                        paint.Color = args.Palette.BorderEmpty;
                    }

                    canvas.DrawPoint(positionsToRender[i].X, positionsToRender[i].Y, paint);
                }
            }
        }

        (_backBuffer, _frontBuffer) = (_frontBuffer, _backBuffer);
        _panningOffset = new();

        return DoneRenderingAsync();
    }

    private Task ResetLogicalAreaAsync()
    {
        _setBoundary = SquareBoundary.GetLargestCenteredSquareInside(PixelBounds.Width, PixelBounds.Height);
        return RequestRenderAsync(RenderInstructions.Everything(PixelBounds));
    }

    protected override Size MeasureOverride(Size availableSize) => availableSize;

    #region State Machine

    private async Task RequestRenderAsync(RenderInstructions instructions)
    {
        var args = new RenderingArgs(
            instructions,
            _setBoundary, 
            Lookup, 
            Palette, 
            ViewPort, 
            RenderInteriors, 
            MinimumIterations, 
            MaximumIterations);

        switch (_state)
        {
            case RenderState.Idle:
                _currentFrameArgs = args;
                await StartRenderingAsync(args);
                _state = RenderState.Rendering;
                IsBusy = true;
                break;

            case RenderState.Rendering:
                if (args != _currentFrameArgs && args != _nextFrameArgs)
                {
                    _nextFrameArgs = args;
                }

                break;
        }
    }

    private async Task DoneRenderingAsync()
    {
        InvalidateVisual();
        _currentFrameArgs = null;

        if (_nextFrameArgs != null)
        {
            _currentFrameArgs = _nextFrameArgs;
            await StartRenderingAsync(_nextFrameArgs);
            _nextFrameArgs = null;
            _state = RenderState.Rendering;
        }
        else
        {
            _state = RenderState.Idle;
            await Dispatcher.UIThread.InvokeAsync(() => IsBusy = false);
        }
    }

    private Task StartRenderingAsync(RenderingArgs args)
    {
        _renderingTask = Task.Run(() => RenderToBufferAsync(args, _cancelSource.Token));
        return Task.CompletedTask;
    }

    private async Task CancelRenderingAsync()
    {
        _cancelSource.Cancel();
        await _renderingTask;
        _cancelSource = new();
    }

    #endregion
}