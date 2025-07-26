using System;
using Avalonia;
using Avalonia.ReactiveUI;
using BoundaryExplorer.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BoundaryExplorer;

static class Program
{
	public static IServiceProvider ServiceProvider { get; } = CreateServiceProvider();

	private static IServiceProvider CreateServiceProvider()
	{
		var services = new ServiceCollection();
		services.ConfigureLogging();
		return services.BuildServiceProvider();
	}

	// Initialization code. Don't use any Avalonia, third-party APIs or any
	// SynchronizationContext-reliant code before AppMain is called: things aren't initialized
	// yet and stuff might break.
	[STAThread]
	public static void Main(string[] args)
	{
		try
		{
			BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
		}
		finally
		{
			// Ensure logs are flushed when the application exits
			LoggingService.CloseAndFlush();
		}
	}

	// Avalonia configuration, don't remove; also used by visual designer.
	// ReSharper disable once MemberCanBePrivate.Global
	public static AppBuilder BuildAvaloniaApp() =>
		AppBuilder.Configure<App>().UsePlatformDetect().LogToTrace().UseReactiveUI();
}
