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

        var topLeft = new Point(0, 0);
        var topRight = new Point(Bounds.Width, 0);
        var bottomLeft = new Point(0, Bounds.Height);
        var bottomRight = new Point(Bounds.Width, Bounds.Height);

        context.DrawLine(new Pen(Brushes.Green), topLeft, bottomRight);
    }

    private void ResetLogicalArea()
    {
        var realRange = new Range(-2, 2);

        var ratio = Height / Width;

        var imagRange = new Range(-(4 * ratio), 0);

        LogicalArea = new ComplexArea(realRange, imagRange);
    }

    protected override Size MeasureOverride(Size availableSize) => availableSize;
}