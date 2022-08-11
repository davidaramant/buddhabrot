﻿using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using BoundaryFinder.Models;
using Buddhabrot.Core.Boundary;
using Buddhabrot.Core.Boundary.RegionQuadTree;
using ReactiveUI;

namespace BoundaryFinder.ViewModels;

public sealed class VisualizeViewModel : ViewModelBase
{
    private readonly BorderDataProvider _dataProvider;
    private readonly Action<string> _log;
    private BoundaryParameters _selectedParameters = new(0, 0);
    private readonly ObservableAsPropertyHelper<int> _minimumIterationsCap;
    private int _minimumIterations = 0;
    private int _numberOfRegions;
    private RegionMap _regions = RegionMap.Empty;

    public ObservableCollection<BoundaryParameters> SavedBoundaries => _dataProvider.SavedBoundaries;

    public BoundaryParameters SelectedParameters
    {
        get => _selectedParameters;
        set => this.RaiseAndSetIfChanged(ref _selectedParameters, value);
    }

    public int MinimumIterationsCap => _minimumIterationsCap.Value;

    public int MinimumIterations
    {
        get => _minimumIterations;
        set => this.RaiseAndSetIfChanged(ref _minimumIterations, value);
    }

    public int NumberOfRegions
    {
        get => _numberOfRegions;
        private set => this.RaiseAndSetIfChanged(ref _numberOfRegions, value);
    }

    public RegionMap Regions
    {
        get => _regions;
        private set => this.RaiseAndSetIfChanged(ref _regions, value);
    }

    public VisualizeViewModel(BorderDataProvider dataProvider, Action<string> log)
    {
        _dataProvider = dataProvider;
        _log = log;

        var loadRegionsCommand = ReactiveCommand.CreateFromTask<BoundaryParameters>(LoadRegionsAsync);

        this.WhenAnyValue(x => x.SelectedParameters,
                bp => bp?.MaxIterations - 1 ?? 0)
            .ToProperty(this, x => x.MinimumIterationsCap, out _minimumIterationsCap);

        this.WhenAnyValue(x => x.SelectedParameters)
            .InvokeCommand(loadRegionsCommand);
    }

    private async Task LoadRegionsAsync(BoundaryParameters parameters, CancellationToken cancelToken)
    {
        var regions = _dataProvider.LoadRegions(SelectedParameters);

        NumberOfRegions = regions.Count;

        var regionMap = await Task.Run(() =>
            new RegionMap(SelectedParameters.VerticalDivisionsPower, regions, log: _log), cancelToken);

        Regions = regionMap;
    }
}