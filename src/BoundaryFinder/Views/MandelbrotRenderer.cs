using System.Drawing;
using System.Reactive;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Buddhabrot.Core;
using Buddhabrot.Core.Boundary;
using ReactiveUI;
using Brushes = Avalonia.Media.Brushes;
using Point = Avalonia.Point;

namespace BoundaryFinder.Views;

public sealed class MandelbrotRenderer : Control
{
    private bool _panning;
    private Point _panningStartPoint;
    private SquareBoundary _panningStart;

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
            if (e.Property.Name == nameof(Lookup) && Lookup.NodeCount > 1)
            {
                ResetLogicalArea();
            }
        };

        ResetViewCommand = ReactiveCommand.Create(ResetLogicalArea);
        ZoomOutCommand = ReactiveCommand.Create(() =>
        {
            SetBoundary = SetBoundary.ZoomOut((int) Bounds.Width, (int) Bounds.Height);
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
            SetBoundary = SetBoundary.ZoomIn((int) pos.X, (int) pos.Y);
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
            var deltaX = (int) (currentPos.X - _panningStartPoint.X);
            var deltaY = (int) (currentPos.Y - _panningStartPoint.Y);

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
        ViewPort = ViewPort.FromResolution(
            new System.Drawing.Size((int)Bounds.Width, (int)Bounds.Height),
            SetBoundary.Center, 
            2d / SetBoundary.QuadrantLength);
        context.FillRectangle(Brushes.LightGray, new Rect(Bounds.Size));

        var center = SetBoundary.Center;
        var radius = SetBoundary.QuadrantLength;
        context.DrawEllipse(Brushes.White, null, new Point(center.X, center.Y), radius, radius);

        var areasToDraw =
            Lookup.GetVisibleAreas(SetBoundary, new Rectangle(0, 0, (int) Bounds.Width, (int) Bounds.Height));
        for (var index = 0; index < areasToDraw.Count; index++)
        {
            var (area, type) = areasToDraw[index];
            var brush = type switch
            {
                RegionType.Border => Brushes.DarkSlateBlue,
                RegionType.Filament => Brushes.Red,
                _ => Brushes.White,
            };
            context.FillRectangle(brush, new Rect(area.X, area.Y, area.Width, area.Height));
        }
    }

    private void ResetLogicalArea() => 
        SetBoundary = SquareBoundary.GetLargestCenteredSquareInside((int) Bounds.Width, (int) Bounds.Height);

    protected override Avalonia.Size MeasureOverride(Avalonia.Size availableSize) => availableSize;
}