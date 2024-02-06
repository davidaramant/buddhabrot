using System;
using System.Reactive;
using System.Runtime.InteropServices;
using BoundaryExplorer.Models;
using Buddhabrot.Core.Utilities;
using CliWrap;
using ReactiveUI;

namespace BoundaryExplorer.ViewModels;

public sealed class SettingsViewModel : ViewModelBase
{
	private string _dataSetPath;
	private string _systemLog = string.Empty;

	public string DataSetPath
	{
		get => _dataSetPath;
		set => this.RaiseAndSetIfChanged(ref _dataSetPath, value);
	}

	public ReactiveCommand<Unit, Unit> UpdateFilePathCommand { get; }

	public ReactiveCommand<Unit, Unit> OpenFilePathCommand { get; }

	public string SystemInformation { get; }

	public string SystemLogOutput
	{
		get => _systemLog;
		set => this.RaiseAndSetIfChanged(ref _systemLog, value);
	}

	public SettingsViewModel(BorderDataProvider dataProvider)
	{
		_dataSetPath = dataProvider.LocalDataStoragePath;

		UpdateFilePathCommand = ReactiveCommand.Create(() =>
		{
			dataProvider.UpdateDataStoragePath(_dataSetPath);
		});

		SystemInformation = ComputerDescription.GetMultiline();

		OpenFilePathCommand = ReactiveCommand.CreateFromTask(
			() =>
				Cli.Wrap(GetCommandToOpenDirectory())
					.WithArguments(".")
					.WithWorkingDirectory(DataSetPath)
					.ExecuteAsync()
		);
	}

	private static string GetCommandToOpenDirectory()
	{
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			return "explorer";
		}
		else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
		{
			return "open";
		}
		else
		{
			throw new Exception("Haven't supported this OS yet");
		}
	}
}
