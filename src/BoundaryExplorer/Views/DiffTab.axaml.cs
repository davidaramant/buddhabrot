using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace BoundaryExplorer.Views;

public partial class DiffTab : UserControl
{
    public DiffTab()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
