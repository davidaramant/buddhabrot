using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using BoundaryFinder.Models;
using Buddhabrot.Core.Boundary;
using Buddhabrot.Core.Boundary.RegionQuadTree;
using DynamicData.Binding;
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
    private IRegionMap _regions = RegionMap2.Empty;
    private bool _isLoadingBoundary;

    public ObservableCollection<BoundaryParameters> SavedBoundaries => _dataProvider.SavedBoundaries;

    public BoundaryParameters SelectedParameters
    {
        get => _selectedParameters;
        set => this.RaiseAndSetIfChanged(ref _selectedParameters, value);
    }

    public bool IsLoadingBoundary
    {
        get => _isLoadingBoundary;
        private set => this.RaiseAndSetIfChanged(ref _isLoadingBoundary, value);
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

    public IRegionMap Regions
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

        this.WhenPropertyChanged(x => x.SelectedParameters, notifyOnInitialValue: false)
            .Where(x => x.Value != null)
            .Select(x => x.Value!)
            .InvokeCommand(loadRegionsCommand);
    }

    private async Task LoadRegionsAsync(BoundaryParameters parameters, CancellationToken cancelToken)
    {
        try
        {
            IsLoadingBoundary = true;
            var regions = _dataProvider.LoadRegions(SelectedParameters);

            NumberOfRegions = regions.Count;

            _log("Creating quad tree...");
            var timer = Stopwatch.StartNew();
            var regionMap = await Task.Run(() =>
                new RegionMap2(SelectedParameters.VerticalDivisionsPower, regions, log: _log), cancelToken);
            _log($"Constructed quad tree in {timer.Elapsed}");

            Regions = regionMap;
        }
        catch (Exception e)
        {
            _log(e.ToString());
        }
        finally
        {
            IsLoadingBoundary = false;
        }
    }
}