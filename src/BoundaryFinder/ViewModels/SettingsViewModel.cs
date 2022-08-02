using System;
using System.IO;
using System.Reactive;
using Buddhabrot.Core.DataStorage;
using ReactiveUI;

namespace BoundaryFinder.ViewModels;

public sealed class SettingsViewModel : ViewModelBase
{
    private readonly string _defaultDataSetPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), 
        "Buddhabrot",
        "Mandelbrot Set Boundaries");
    
    private string _dataSetPath;

    public string DataSetPath
    {
        get => _dataSetPath;
        set => this.RaiseAndSetIfChanged(ref _dataSetPath, value);
    }

    public ReactiveCommand<Unit, Unit> UpdateFilePathCommand { get; }
    
    public SettingsViewModel(DataProvider dataProvider, Action<string> log)
    {
        if (string.IsNullOrWhiteSpace(dataProvider.LocalDataStoragePath))
        {
            dataProvider.LocalDataStoragePath = _defaultDataSetPath;
        }

        _dataSetPath = dataProvider.LocalDataStoragePath;
        
        UpdateFilePathCommand = ReactiveCommand.Create(() =>
        {
            dataProvider.LocalDataStoragePath = _dataSetPath;
        });
    }
}