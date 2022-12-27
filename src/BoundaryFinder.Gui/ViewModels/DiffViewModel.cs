using System;
using System.Collections.ObjectModel;
using System.Reactive;
using BoundaryFinder.Gui.Models;
using Buddhabrot.Core.Boundary;
using Buddhabrot.Core.DataStorage;
using ReactiveUI;

namespace BoundaryFinder.Gui.ViewModels;

public sealed class DiffViewModel : ViewModelBase
{
    private readonly BorderDataProvider _dataProvider;
    private readonly Action<string> _log;
    private BoundaryDataSet _selectedLeft = BoundaryDataSet.Empty;
    private BoundaryDataSet _selectedRight = BoundaryDataSet.Empty;

    public ObservableCollection<BoundaryDataSet> LeftDataSets { get; } = new();
    public ObservableCollection<BoundaryDataSet> RightDataSets { get; } = new();
    
    public BoundaryDataSet SelectedLeft
    {
        get => _selectedLeft;
        set => this.RaiseAndSetIfChanged(ref _selectedLeft, value);
    }
    
    public BoundaryDataSet SelectedRight
    {
        get => _selectedRight;
        set => this.RaiseAndSetIfChanged(ref _selectedRight, value);
    }
    
    public ReactiveCommand<Unit, Unit> ComputeDiffCommand { get; }

    public DiffViewModel(BorderDataProvider dataProvider, Action<string> log)
    {
        _dataProvider = dataProvider;
        _log = log;
        
        ComputeDiffCommand = ReactiveCommand.Create(ComputeDiff);
    }

    private void ComputeDiff()
    {
        var left = _dataProvider.LoadLookup(SelectedLeft);
        var right = _dataProvider.LoadLookup(SelectedRight);

        var diff = RegionLookupDiffer.Diff(left, right);
        
        _dataProvider.SaveDiff(SelectedLeft.Parameters, SelectedRight.Parameters, diff);
    }
}