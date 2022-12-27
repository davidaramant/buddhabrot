using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using BoundaryFinder.Gui.Models;
using Buddhabrot.Core.Boundary;
using Buddhabrot.Core.Boundary.Visualization;
using Buddhabrot.Core.DataStorage;
using DynamicData.Binding;
using ReactiveUI;

namespace BoundaryFinder.Gui.ViewModels;

public sealed class VisualizeViewModel : ViewModelBase
{
    private readonly BorderDataProvider _dataProvider;
    private readonly Action<string> _log;
    private BoundaryDataSet _selectedParameters = BoundaryDataSet.Empty;
    private int _minIterations = 0;
    private int _maxIterations = 100_000;
    private RegionLookup _lookup = RegionLookup.Empty;
    
    public ObservableCollection<BoundaryDataSet> SavedBoundaries => _dataProvider.SavedBoundaries;

    public BoundaryDataSet SelectedParameters
    {
        get => _selectedParameters;
        set => this.RaiseAndSetIfChanged(ref _selectedParameters, value);
    }

    public int MinIterations
    {
        get => _minIterations;
        set => this.RaiseAndSetIfChanged(ref _minIterations, value);
    }
    
    public int MaxIterations
    {
        get => _maxIterations;
        set => this.RaiseAndSetIfChanged(ref _maxIterations, value);
    }

    public RegionLookup Lookup
    {
        get => _lookup;
        private set => this.RaiseAndSetIfChanged(ref _lookup, value);
    }

    public ReactiveCommand<Unit, Unit> SaveQuadTreeRenderingCommand { get; }

    public VisualizeViewModel(BorderDataProvider dataProvider, Action<string> log)
    {
        _dataProvider = dataProvider;
        _log = log;

        var loadLookupCommand = ReactiveCommand.Create<BoundaryDataSet>(LoadLookup);

        this.WhenPropertyChanged(x => x.SelectedParameters, notifyOnInitialValue: false)
            .Where(x => x.Value != null)
            .Select(x => x.Value!)
            .InvokeCommand(loadLookupCommand);

        SaveQuadTreeRenderingCommand = ReactiveCommand.Create(SaveQuadTreeRendering);
    }

    private void SaveQuadTreeRendering()
    {
        try
        {
            using var img = BoundaryVisualizer.RenderRegionLookup(Lookup);
            img.Save(System.IO.Path.Combine(_dataProvider.LocalDataStoragePath, SelectedParameters.Description + ".png"));
        }
        catch (Exception e)
        {
            _log(e.ToString());
        }
    }

    private void LoadLookup(BoundaryDataSet parameters)
    {
        try
        {
            var lookup = _dataProvider.LoadLookup(SelectedParameters);

            Lookup = lookup;
            MinIterations = SelectedParameters.Parameters.MaxIterations / 10;
            MaxIterations = SelectedParameters.Parameters.MaxIterations;
            _log($"Quad tree nodes: {Lookup.NodeCount:N0}");
        }
        catch (Exception e)
        {
            _log(e.ToString());
        }
    }
}