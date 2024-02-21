using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace BoundaryExplorer.Views;

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
