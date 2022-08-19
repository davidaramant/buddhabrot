using System.Numerics;
using System.Reactive;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Buddhabrot.Core;
using Buddhabrot.Core.Boundary;
using ReactiveUI;

namespace BoundaryFinder.Views;

public sealed class MandelbrotRenderer : Control
{
    private bool _panning = false;
    private Point _panningStartPoint = new Point();

    public static readonly StyledProperty<ComplexArea> LogicalAreaProperty =
        AvaloniaProperty.Register<MandelbrotRenderer, ComplexArea>(nameof(LogicalArea));

    public ComplexArea LogicalArea
    {
        get => GetValue(LogicalAreaProperty);
        set => SetValue(LogicalAreaProperty, value);
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
        AffectsRender<MandelbrotRenderer>(LogicalAreaProperty);
    }

    public MandelbrotRenderer()
    {
        ClipToBounds = true;
        // HACK: I'm sure there is some fancy Reactive way to do this
        this.PropertyChanged += (s, e) =>
        {
            if (e.Property.Name == nameof(Lookup) && Lookup != null)
            {
                ResetLogicalArea();
            }
        };

        ResetViewCommand = ReactiveCommand.Create(ResetLogicalArea);
        ZoomOutCommand = ReactiveCommand.Create(() => { LogicalArea = LogicalArea.Scale(1.25); });
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (e.ClickCount == 1)
        {
            _panning = true;
            _panningStartPoint = e.GetPosition(this);
        }
        else if (e.ClickCount == 2)
        {
            _panning = false;
            LogicalArea = LogicalArea.Scale(0.75);
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
            // HACK: This seems weird, but it's functional enough
            var dampingFactor = 0.03d;
            var currentPos = e.GetPosition(this);
            var deltaX = dampingFactor * (currentPos.X - _panningStartPoint.X);
            var deltaY = dampingFactor * (currentPos.Y - _panningStartPoint.Y);

            var viewPort = GetCurrentViewPort();

            LogicalArea = LogicalArea.OffsetBy(-deltaX * viewPort.RealPixelSize, deltaY * viewPort.ImagPixelSize);
        }

        base.OnPointerMoved(e);
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        _panning = false;
        base.OnPointerCaptureLost(e);
    }

    private ViewPort GetCurrentViewPort() =>
        new(LogicalArea, new System.Drawing.Size((int)Bounds.Width, (int)Bounds.Height));

    public override void Render(DrawingContext context)
    {
        context.FillRectangle(Brushes.LightGray, new Rect(Bounds.Size));
        AdjustLogicalArea(Bounds.Size);
        var viewPort = GetCurrentViewPort();

        var center = viewPort.GetPosition(Complex.Zero);
        var radius = 2 / viewPort.RealPixelSize;
        context.DrawEllipse(Brushes.White, null, new Point(center.X, center.Y), radius, radius);

        var areasToDraw = Lookup.GetVisibleAreas(LogicalArea, viewPort.RealPixelSize);
        for (var index = 0; index < areasToDraw.Count; index++)
        {
            var area = areasToDraw[index];
            var rect = viewPort.GetRectangle(area);
            context.FillRectangle(Brushes.DarkSlateBlue, new Rect(rect.X, rect.Y, rect.Width, rect.Height));
        }
    }

    private void ResetLogicalArea()
    {
        var defaultView = new ComplexArea(new Range(-2, 2), new Range(-2, 2));
        var viewRatio = Bounds.Width / Bounds.Height;

        if (viewRatio >= 1)
        {
            // Use vertical, pad horizontal
            var realPaddedMagnitude = defaultView.RealRange.Magnitude * viewRatio;
            LogicalArea = defaultView with
            {
                RealRange = Range.FromCenterAndLength(0, realPaddedMagnitude)
            };
        }
        else
        {
            // Use horizontal, pad vertical
            var imagPaddedMagnitude = defaultView.ImagRange.Magnitude / viewRatio;
            LogicalArea = defaultView with
            {
                ImagRange = Range.FromCenterAndLength(0, imagPaddedMagnitude)
            };
        }
    }

    private void AdjustLogicalArea(Size bounds)
    {
        var aspectRatio = bounds.Width / bounds.Height;
        var imagMagnitude = LogicalArea.RealRange.Magnitude / aspectRatio;

        LogicalArea = LogicalArea with
        {
            ImagRange = Range.FromMinAndLength(LogicalArea.ImagRange.InclusiveMin, imagMagnitude)
        };
    }

    protected override Size MeasureOverride(Size availableSize) => availableSize;
}