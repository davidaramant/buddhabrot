using System;
using System.IO;
using System.Reactive;
using BoundaryFinder.Models;
using ReactiveUI;

namespace BoundaryFinder.ViewModels;

public sealed class ConfigureDataSourceViewModel : ViewModelBase
{
    private string _dataSetPath;

    public string DataSetPath
    {
        get => _dataSetPath;
        set => this.RaiseAndSetIfChanged(ref _dataSetPath, value);
    }

    public ReactiveCommand<Unit, Unit> LoadFileData { get; }
    
    public ConfigureDataSourceViewModel(DataSourceManager dataSourceManager)
    {
        _dataSetPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), 
            "Buddhabrot",
            "Mandelbrot Set Boundaries");
        
        LoadFileData = ReactiveCommand.Create(() => dataSourceManager.SetUpFileSystemProvider(_dataSetPath));
    }
}