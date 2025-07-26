using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using BoundaryExplorer.ViewModels;
using BoundaryExplorer.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BoundaryExplorer;

public class App : Application
{
	private ILogger<App>? _logger;

	public override void OnFrameworkInitializationCompleted()
	{
		_logger = Program.ServiceProvider.GetRequiredService<ILogger<App>>();

		if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
		{
			_logger.LogInformation("Creating main window");
			desktop.MainWindow = new MainWindow { DataContext = new MainWindowViewModel() };
		}

		base.OnFrameworkInitializationCompleted();
	}
}
