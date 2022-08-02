using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive;
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

    public BorderDataViewModel(DataProvider dataProvider, Action<string> log)
    {
        _dataProvider = dataProvider;
        _log = log;
        LoadDataSetsCommand = ReactiveCommand.Create(LoadDataSets);
        SelectDataSetCommand = ReactiveCommand.Create(SelectDataSet);
        RenderBorderRegionsCommand = ReactiveCommand.Create(RenderBoundary);
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

    private void RenderBoundary()
    {
        try
        {
            using var img = BoundaryVisualizer.RenderBorderRegions(_regions);

            var dirPath = _dataProvider.GetBoundaryParameterLocation(SelectedParameters!);

            img.Save(Path.Combine(dirPath, SelectedParameters!.Description + ".png"));
        }
        catch (Exception e)
        {
            _log(e.ToString());
        }
    }
}