using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Buddhabrot.Core;
using Buddhabrot.Core.Boundary.RegionQuadTree;
using ReactiveUI;

namespace BoundaryFinder.Views;

public sealed class MandelbrotRenderer : Control
{
    static MandelbrotRenderer()
    {
        AffectsRender<MandelbrotRenderer>(RegionsProperty);
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
    }

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

    public override void Render(DrawingContext context)
    {
        context.FillRectangle(Brushes.White, new Rect(Bounds.Size));
        AdjustLogicalArea(Bounds.Size);
        var width = Bounds.Width;
        var height = Bounds.Height;

        var viewPort = new ViewPort(LogicalArea, new System.Drawing.Size((int) width, (int) height));

        var areasToDraw = Regions.GetVisibleAreas(LogicalArea);
        foreach (var area in areasToDraw)
        {
            var rect = viewPort.GetRectangle(area);

            context.FillRectangle(Brushes.Red, new Rect(rect.X, rect.Y, rect.Width, rect.Height));
        }
    }

    private void ResetLogicalArea()
    {
        var realRange = new Range(-2, 2);

        var ratio = Bounds.Height / Bounds.Width;

        var imagRange = new Range(-(4 * ratio), 0);

        LogicalArea = new ComplexArea(realRange, imagRange);
    }

    private void AdjustLogicalArea(Size bounds)
    {
        var ratio = bounds.Height / bounds.Width;
        var imagMagnitude = LogicalArea.RealRange.Magnitude * ratio;

        LogicalArea = LogicalArea with
        {
            ImagRange = Range.FromMinAndLength(LogicalArea.ImagRange.ExclusiveMax - imagMagnitude, imagMagnitude)
        };
    }

    protected override Size MeasureOverride(Size availableSize) => availableSize;
}