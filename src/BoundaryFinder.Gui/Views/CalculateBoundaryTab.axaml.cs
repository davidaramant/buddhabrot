using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace BoundaryFinder.Gui.Views;

public partial class CalculateBoundaryTab : UserControl
{
    public CalculateBoundaryTab()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}