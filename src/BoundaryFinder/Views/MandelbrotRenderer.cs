using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Buddhabrot.Core;

namespace BoundaryFinder.Views;

public sealed class MandelbrotRenderer : Control
{
    static MandelbrotRenderer()
    {
        AffectsRender<MandelbrotRenderer>(LogicalAreaProperty);
    }

    public static readonly StyledProperty<ComplexArea> LogicalAreaProperty =
        AvaloniaProperty.Register<MandelbrotRenderer, ComplexArea>(nameof(LogicalArea));

    public ComplexArea LogicalArea
    {
        get => GetValue(LogicalAreaProperty);
        set => SetValue(LogicalAreaProperty, value);
    }
    
    public override void Render(DrawingContext context)
    {
        context.FillRectangle(Brushes.Aqua,new Rect(Bounds.Size));
        
        var topLeft = new Point(0, 0);
        var topRight = new Point(Bounds.Width, 0);
        var bottomLeft = new Point(0, Bounds.Height);
        var bottomRight = new Point(Bounds.Width, Bounds.Height);

        context.DrawLine(new Pen(Brushes.Green), topLeft, bottomRight);
    }

    protected override Size MeasureOverride(Size availableSize) => availableSize;
}