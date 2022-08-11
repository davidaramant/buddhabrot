﻿using System;
using Avalonia.Threading;
using BoundaryFinder.Models;
using Buddhabrot.Core.DataStorage;
using ReactiveUI;

namespace BoundaryFinder.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly BorderDataProvider _dataProvider = new(new DataProvider());
    private string _logOutput = string.Empty;

    public VisualizeViewModel Visualize { get; }
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
        Visualize = new VisualizeViewModel(_dataProvider, Log);
        Boundaries = new BoundariesViewModel(_dataProvider, Log);
        CalculateBoundary = new CalculateBoundaryViewModel(_dataProvider, Log);
        Settings = new SettingsViewModel(_dataProvider, Log);
    }
    
    private void Log(string msg) => Dispatcher.UIThread.Post(() => LogOutput += msg + Environment.NewLine);
}