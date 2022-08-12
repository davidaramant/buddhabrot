using System.Reactive;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Buddhabrot.Core;
using Buddhabrot.Core.Boundary.RegionQuadTree;
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

    public static readonly StyledProperty<RegionMap> RegionsProperty =
        AvaloniaProperty.Register<MandelbrotRenderer, RegionMap>(nameof(Regions));

    public RegionMap Regions
    {
        get => GetValue(RegionsProperty);
        set => SetValue(RegionsProperty, value);
    }

    public ReactiveCommand<Unit, Unit> ResetViewCommand { get; }
    public ReactiveCommand<Unit, Unit> ZoomOutCommand { get; }

    static MandelbrotRenderer()
    {
        AffectsRender<MandelbrotRenderer>(RegionsProperty);
        AffectsRender<MandelbrotRenderer>(LogicalAreaProperty);
    }

    public MandelbrotRenderer()
    {
        // HACK: I'm sure there is some fancy Reactive way to do this
        this.PropertyChanged += (s, e) =>
        {
            if (e.Property.Name == nameof(Regions))
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
        new(LogicalArea, new System.Drawing.Size((int) Bounds.Width, (int) Bounds.Height));

    public override void Render(DrawingContext context)
    {
        context.FillRectangle(Brushes.White, new Rect(Bounds.Size));
        AdjustLogicalArea(Bounds.Size);
        var viewPort = GetCurrentViewPort();

        var areasToDraw = Regions.GetVisibleAreas(LogicalArea);
        foreach (var area in areasToDraw)
        {
            var rect = viewPort.GetRectangle(area);

            context.FillRectangle(Brushes.Red, new Rect(rect.X, rect.Y, rect.Width, rect.Height));
        }
    }

    private void ResetLogicalArea()
    {
        var populatedArea = Regions.PopulatedArea;
        var populatedAreaRatio = populatedArea.RealRange.Magnitude / populatedArea.ImagRange.Magnitude;
        var viewRatio = Bounds.Width / Bounds.Height;

        if (viewRatio >= populatedAreaRatio)
        {
            // Use vertical, pad horizontal
            var realPaddedMagnitude = populatedArea.RealRange.Magnitude * (viewRatio / populatedAreaRatio);
            LogicalArea = populatedArea with
            {
                RealRange = Range.FromMinAndLength(-2, realPaddedMagnitude)
            };
        }
        else
        {
            // Use horizontal, pad vertical
            var imagPaddedMagnitude = populatedArea.ImagRange.Magnitude * (populatedAreaRatio / viewRatio);
            LogicalArea = populatedArea with
            {
                ImagRange = new Range(-imagPaddedMagnitude, 0)
            };
        }
    }

    private void AdjustLogicalArea(Size bounds)
    {
        var aspectRatio = bounds.Width / bounds.Height;
        var imagMagnitude = LogicalArea.RealRange.Magnitude / aspectRatio;
        
        LogicalArea = LogicalArea with
        {
            ImagRange = new Range(LogicalArea.ImagRange.ExclusiveMax - imagMagnitude, LogicalArea.ImagRange.ExclusiveMax)
        };
    }

    protected override Size MeasureOverride(Size availableSize) => availableSize;
}