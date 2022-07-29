using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using BoundaryFinder.Models;
using Buddhabrot.Core.Boundary;
using DynamicData;
using ReactiveUI;

namespace BoundaryFinder.ViewModels;

public sealed class BorderDataViewModel : ViewModelBase
{
    private readonly DataSourceManager _dataSourceManager;
    private BoundaryParameters? _selectedParameters;
    private int _verticalDivisions;
    private IReadOnlyList<RegionId> _regions = Array.Empty<RegionId>();
    private int _numberOfRegions;

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

    public ReactiveCommand<Unit, Unit> RenderBorderRegionsCommand { get; }

    public BorderDataViewModel(DataSourceManager dataSourceManager)
    {
        _dataSourceManager = dataSourceManager;
        LoadDataSetsCommand = ReactiveCommand.CreateFromTask(LoadDataSetsAsync);
        SelectDataSetCommand = ReactiveCommand.CreateFromTask(SelectDataSetAsync);
        RenderBorderRegionsCommand = ReactiveCommand.Create(RenderBoundary);
    }

    private async Task LoadDataSetsAsync()
    {
        Boundaries.Clear();

        var dataSets = await _dataSourceManager.DataProvider.GetBoundaryParametersAsync();

        Boundaries.AddRange(dataSets);
    }

    private async Task SelectDataSetAsync()
    {
        if (SelectedParameters != null)
        {
            _regions = await _dataSourceManager.DataProvider.GetBoundaryRegionsAsync(SelectedParameters);
            VerticalDivisions = SelectedParameters.VerticalDivisions;
            NumberOfRegions = _regions.Count;
        }
    }

    private void RenderBoundary()
    {
        using var img = BoundaryVisualizer.RenderBorderRegions(_regions);
        
        // TODO: This should be saved in the local data path
        img.Save(SelectedParameters!.ToFilePrefix() + ".png");
    }
}