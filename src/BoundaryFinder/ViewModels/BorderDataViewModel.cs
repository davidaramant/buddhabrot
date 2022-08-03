using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using BoundaryFinder.RandomExtensions;
using Buddhabrot.Core;
using Buddhabrot.Core.Boundary;
using Buddhabrot.Core.DataStorage;
using DynamicData;
using ReactiveUI;

namespace BoundaryFinder.ViewModels;

public sealed class BorderDataViewModel : ViewModelBase
{
    private readonly DataProvider _dataProvider;
    private readonly Action<string> _log;
    private BoundaryParameters? _selectedParameters;
    private int _verticalDivisions;
    private IReadOnlyList<RegionId> _regions = Array.Empty<RegionId>();
    private int _numberOfRegions;
    private int _minimumIterations = 5_000_000;
    private int _regionImageSizeMultiplier = 10;
    private readonly ObservableAsPropertyHelper<int> _regionImageSize;
    private int _numRegionsToRender = 1;
    private int _numRegionsRendered = 0;

    public ReactiveCommand<Unit, Unit> LoadDataSetsCommand { get; }

    public ObservableCollection<BoundaryParameters> Boundaries { get; } = new();

    public BoundaryParameters? SelectedParameters
    {
        get => _selectedParameters;
        set => this.RaiseAndSetIfChanged(ref _selectedParameters, value);
    }

    public ReactiveCommand<Unit, Unit> SelectDataSetCommand { get; }

    public int VerticalDivisions
    {
        get => _verticalDivisions;
        private set => this.RaiseAndSetIfChanged(ref _verticalDivisions, value);
    }

    public int NumberOfRegions
    {
        get => _numberOfRegions;
        private set => this.RaiseAndSetIfChanged(ref _numberOfRegions, value);
    }

    public ReactiveCommand<Unit, Unit> RenderBoundaryCommand { get; }

    public int RegionImageSizeMultiplier
    {
        get => _regionImageSizeMultiplier;
        set => this.RaiseAndSetIfChanged(ref _regionImageSizeMultiplier, value);
    }

    public int RegionImageSize => _regionImageSize.Value;

    public int MinimumIterations
    {
        get => _minimumIterations;
        set => this.RaiseAndSetIfChanged(ref _minimumIterations, value);
    }

    public ReactiveCommand<Unit, Unit> RenderRegionsCommand { get; }

    public int NumRegionsToRender
    {
        get => _numRegionsToRender;
        set => this.RaiseAndSetIfChanged(ref _numRegionsToRender, value);
    }

    public int NumRegionsRendered
    {
        get => _numRegionsRendered;
        set => this.RaiseAndSetIfChanged(ref _numRegionsRendered, value);
    }

    public BorderDataViewModel(DataProvider dataProvider, Action<string> log)
    {
        _dataProvider = dataProvider;
        _log = log;
        LoadDataSetsCommand = ReactiveCommand.Create(LoadDataSets);
        SelectDataSetCommand = ReactiveCommand.Create(SelectDataSet);
        RenderBoundaryCommand = ReactiveCommand.CreateFromTask(RenderBoundaryAsync);

        this.WhenAnyValue(x => x.RegionImageSizeMultiplier, multiplier => multiplier * 100)
            .ToProperty(this, x => x.RegionImageSize, out _regionImageSize);

        RenderRegionsCommand = ReactiveCommand.CreateFromTask(RenderRegionsAsync);
    }

    private void LoadDataSets()
    {
        Boundaries.Clear();

        var dataSets = _dataProvider.GetBoundaryParameters();

        Boundaries.AddRange(dataSets);
    }

    private void SelectDataSet()
    {
        if (SelectedParameters != null)
        {
            _regions = _dataProvider.GetBoundaryRegions(SelectedParameters);
            VerticalDivisions = SelectedParameters.VerticalDivisions;
            NumberOfRegions = _regions.Count;
        }
    }

    private Task RenderBoundaryAsync() =>
        Task.Run(() =>
        {
            try
            {
                using var img = BoundaryVisualizer.RenderBoundary(_regions);

                img.Save(Path.Combine(_dataProvider.LocalDataStoragePath, SelectedParameters!.Description + ".png"));
            }
            catch (Exception e)
            {
                _log(e.ToString());
            }
        });

    private Task RenderRegionsAsync()
    {
        return Task.Run(() =>
        {
            try
            {
                var plotParameters = new PlotParameters(SelectedParameters!.VerticalDivisions,
                    new IterationRange(MinimumIterations, SelectedParameters!.MaxIterations));

                var dirPath = _dataProvider.GetBoundaryParameterLocation(SelectedParameters!);

                var random = new Random();

                const int samples = 10;
                NumRegionsToRender = samples;
                NumRegionsRendered = 0;
                
                var stopwatch = Stopwatch.StartNew();
                
                foreach (var region in _regions.GetRandomSequence(random).Take(samples))
                {
                    _log($"Visualizing {region}...");

                    var viewPort = new ViewPort(plotParameters.GetAreaOfId(region),
                        new Size(RegionImageSize, RegionImageSize));

                    using var img = BoundaryVisualizer.RenderBorderRegion(
                        viewPort,
                        plotParameters.IterationRange);

                    img.Save(Path.Combine(dirPath,
                        $"l{MinimumIterations:N0}_x{region.X}_y{region.Y}_size{RegionImageSize}.png"));

                    NumRegionsRendered++;
                }

                _log($"Took {stopwatch.Elapsed}");
            }
            catch (Exception e)
            {
                _log(e.ToString());
            }
        });
    }
}