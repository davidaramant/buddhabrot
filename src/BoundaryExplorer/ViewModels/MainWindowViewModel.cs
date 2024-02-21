using System;
using Avalonia.Threading;
using BoundaryExplorer.Models;
using Buddhabrot.Core.DataStorage;

namespace BoundaryExplorer.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
	private readonly BorderDataProvider _dataProvider = new(new DataProvider());

	public VisualizeViewModel Visualize { get; }
	public CalculateBoundaryViewModel CalculateBoundary { get; }
	public DiffViewModel Diff { get; }
	public SettingsViewModel Settings { get; }

	public MainWindowViewModel()
	{
		Settings = new SettingsViewModel(_dataProvider);
		void LogAction(string msg) =>
			Dispatcher.UIThread.Post(() => Settings.SystemLogOutput += msg + Environment.NewLine);

		Visualize = new VisualizeViewModel(_dataProvider, LogAction);
		CalculateBoundary = new CalculateBoundaryViewModel(_dataProvider, LogAction);
		Diff = new DiffViewModel(_dataProvider, LogAction);
	}
}
