﻿using System;
using System.Collections.ObjectModel;
using System.Reactive;
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
    private BoundaryDataSet? _selectedLeft = null;
    private BoundaryDataSet? _selectedRight = null;
    private readonly ReadOnlyObservableCollection<BoundaryDataSet> _leftDataSets;
    private readonly ReadOnlyObservableCollection<BoundaryDataSet> _rightDataSets;

    public ReadOnlyObservableCollection<BoundaryDataSet> LeftDataSets => _leftDataSets;
    public ReadOnlyObservableCollection<BoundaryDataSet> RightDataSets => _rightDataSets;

    public BoundaryDataSet? SelectedLeft
    {
        get => _selectedLeft;
        set => this.RaiseAndSetIfChanged(ref _selectedLeft, value);
    }

    public BoundaryDataSet? SelectedRight
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

        var canDiff = this.WhenAnyValue(t => t.SelectedLeft, t => t.SelectedRight,
            (left, right) => left != null && right != null);

        ComputeDiffCommand = ReactiveCommand.Create(ComputeDiff, canDiff);
    }

    private void ComputeDiff()
    {
        try
        {
            var left = _dataProvider.LoadLookup(SelectedLeft!);
            var right = _dataProvider.LoadLookup(SelectedRight!);

            var diff = RegionLookupDiffer.Diff(left, right);

            _dataProvider.SaveDiff(SelectedLeft!.Parameters, SelectedRight!.Parameters, diff);
        }
        catch (Exception e)
        {
            _log("Failed creating diff: " + e);
        }
    }
}