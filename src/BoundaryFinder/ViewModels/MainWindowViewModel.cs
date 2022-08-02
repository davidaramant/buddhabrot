using System;
using Avalonia.Threading;
using Buddhabrot.Core.DataStorage;
using ReactiveUI;

namespace BoundaryFinder.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly DataProvider _dataProvider = new();
    private string _logOutput = string.Empty;

    public BorderDataViewModel BorderData { get; }
    public FindNewBoundaryViewModel FindNewBoundary { get; }
    public SettingsViewModel Settings { get; }

    public string LogOutput
    {
        get => _logOutput;
        private set => this.RaiseAndSetIfChanged(ref _logOutput, value);
    }

    public MainWindowViewModel()
    {
        BorderData = new BorderDataViewModel(_dataProvider, Log);
        FindNewBoundary = new FindNewBoundaryViewModel(_dataProvider, Log);
        Settings = new SettingsViewModel(_dataProvider, Log);
    }
    
    private void Log(string msg) => Dispatcher.UIThread.Post(() => LogOutput += msg + Environment.NewLine);
}