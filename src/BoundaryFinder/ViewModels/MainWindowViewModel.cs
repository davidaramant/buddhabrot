using System;
using Avalonia.Threading;
using Buddhabrot.Core.DataStorage;
using ReactiveUI;

namespace BoundaryFinder.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly DataProvider _dataProvider = new();
    private string _logOutput = string.Empty;

    public BoundariesViewModel Boundaries { get; }
    public CalculateBoundaryViewModel CalculateBoundary { get; }
    public SettingsViewModel Settings { get; }

    public string LogOutput
    {
        get => _logOutput;
        private set => this.RaiseAndSetIfChanged(ref _logOutput, value);
    }

    public MainWindowViewModel()
    {
        Boundaries = new BoundariesViewModel(_dataProvider, Log);
        CalculateBoundary = new CalculateBoundaryViewModel(_dataProvider, Log);
        Settings = new SettingsViewModel(_dataProvider, Log);
    }
    
    private void Log(string msg) => Dispatcher.UIThread.Post(() => LogOutput += msg + Environment.NewLine);
}