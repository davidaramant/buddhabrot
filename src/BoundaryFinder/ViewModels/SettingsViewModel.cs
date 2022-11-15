using System;
using System.Reactive;
using System.Runtime.InteropServices;
using BoundaryFinder.Models;
using Buddhabrot.Core.Utilities;
using ReactiveUI;

namespace BoundaryFinder.ViewModels;

public sealed class SettingsViewModel : ViewModelBase
{
    private string _dataSetPath;

    public string DataSetPath
    {
        get => _dataSetPath;
        set => this.RaiseAndSetIfChanged(ref _dataSetPath, value);
    }

    public ReactiveCommand<Unit, Unit> UpdateFilePathCommand { get; }

    public ReactiveCommand<Unit, Unit> OpenFilePathCommand { get; }

    public string SystemInformation { get; }

    public SettingsViewModel(BorderDataProvider dataProvider, Action<string> log)
    {
        _dataSetPath = dataProvider.LocalDataStoragePath;

        UpdateFilePathCommand = ReactiveCommand.Create(() =>
        {
            dataProvider.UpdateDataStoragePath(_dataSetPath);
        });

        SystemInformation = ComputerDescription.Get();

        OpenFilePathCommand = ReactiveCommand.Create(() =>
        {
            System.Diagnostics.Process.Start(GetCommandToOpenDirectory(), DataSetPath);
        });
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