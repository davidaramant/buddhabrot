using System;
using System.Reactive;
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

    public string SystemInformation {get;}
    
    public SettingsViewModel(BorderDataProvider dataProvider, Action<string> log)
    {
        _dataSetPath = dataProvider.LocalDataStoragePath;
        
        UpdateFilePathCommand = ReactiveCommand.Create(() =>
        {
            dataProvider.UpdateDataStoragePath(_dataSetPath);
        });

        SystemInformation = ComputerDescription.Get();
    }
}