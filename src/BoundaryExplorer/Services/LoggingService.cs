using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace BoundaryExplorer.Services;

public static class LoggingService
{
	public static IServiceCollection ConfigureLogging(this IServiceCollection services)
	{
		// Configure Serilog
		var logDirectory = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
			"Buddhabrot",
			"Logs"
		);
		Directory.CreateDirectory(logDirectory);

		var logFilePath = Path.Combine(logDirectory, "BoundaryExplorer.log");

		Log.Logger = new LoggerConfiguration()
			.MinimumLevel.Information()
			.MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
			.Enrich.FromLogContext()
			.WriteTo.File(
				logFilePath,
				rollingInterval: RollingInterval.Day,
				retainedFileCountLimit: 7,
				outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] {Message:lj}{NewLine}{Exception}"
			)
			.CreateLogger();

		// Add Microsoft logging with Serilog
		services.AddLogging(builder =>
		{
			builder.ClearProviders();
			builder.AddSerilog(Log.Logger, dispose: true);
		});

		return services;
	}

	public static void CloseAndFlush()
	{
		Log.CloseAndFlush();
	}
}
