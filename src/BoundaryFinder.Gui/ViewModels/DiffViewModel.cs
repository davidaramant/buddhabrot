using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using BoundaryFinder.Gui.Models;
using Buddhabrot.Core.Boundary;
using Buddhabrot.Core.DataStorage;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;

namespace BoundaryFinder.Gui.ViewModels;

public sealed class DiffViewModel : ViewModelBase
{
    private readonly BorderDataProvider _dataProvider;
    private readonly Action<string> _log;
    private BoundaryDataSet _selectedLeft = BoundaryDataSet.Empty;
    private BoundaryDataSet _selectedRight = BoundaryDataSet.Empty;
    private readonly ReadOnlyObservableCollection<BoundaryDataSet> _leftDataSets;
    private readonly ReadOnlyObservableCollection<BoundaryDataSet> _rightDataSets;

    public ReadOnlyObservableCollection<BoundaryDataSet> LeftDataSets => _leftDataSets;
    public ReadOnlyObservableCollection<BoundaryDataSet> RightDataSets => _rightDataSets;

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

        dataProvider
            .SavedBoundaries
            .Connect()
            .Filter(b => !b.IsDiff)
            //.ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out _leftDataSets)
            .Subscribe();

        var selectedLeftObservable = this.WhenValueChanged(t => t.SelectedLeft);

        _dataProvider
            .SavedBoundaries
            .Connect()
            .AutoRefreshOnObservable(_ => selectedLeftObservable)
            .Filter(b => !b.IsDiff && b != SelectedLeft)
            .Sort(BoundaryDataSet.Comparer)
            //.ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out _rightDataSets)
            .Subscribe();

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