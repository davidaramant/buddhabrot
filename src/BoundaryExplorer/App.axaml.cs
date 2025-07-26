using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using BoundaryExplorer.ViewModels;
using BoundaryExplorer.Views;

namespace BoundaryExplorer;

public class App : Application
{
	public override void OnFrameworkInitializationCompleted()
	{
		if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
		{
			desktop.MainWindow = new MainWindow { DataContext = new MainWindowViewModel() };
		}

		base.OnFrameworkInitializationCompleted();
	}
}
