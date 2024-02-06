using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace BoundaryExplorer.Views;

public partial class VisualizeTab : UserControl
{
	public VisualizeTab()
	{
		InitializeComponent();
	}

	private void InitializeComponent()
	{
		AvaloniaXamlLoader.Load(this);
	}
}
