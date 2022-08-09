using System.Collections.Generic;
using System.Collections.ObjectModel;
using BoundaryFinder.Models;
using Buddhabrot.Core.Boundary;
using ReactiveUI;

namespace BoundaryFinder.ViewModels;

public sealed class VisualizeViewModel : ViewModelBase
{
    private readonly BorderDataProvider _dataProvider;
    private BoundaryParameters? _selectedParameters;
    private readonly ObservableAsPropertyHelper<int> _minimumIterationsCap;
    private int _minimumIterations = 0;
    private readonly ObservableAsPropertyHelper<int> _numberOfRegions;
    private readonly List<RegionId> _regions = new();

    public ObservableCollection<BoundaryParameters> SavedBoundaries => _dataProvider.SavedBoundaries;

    public BoundaryParameters? SelectedParameters
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

    public int NumberOfRegions => _numberOfRegions.Value;

    public VisualizeViewModel(BorderDataProvider dataProvider)
    {
        _dataProvider = dataProvider;

        this.WhenAnyValue(x => x.SelectedParameters,
                bp => bp?.MaxIterations - 1 ?? 0)
            .ToProperty(this, x => x.MinimumIterationsCap, out _minimumIterationsCap);

        // Is this how you're supposed to do side effects?
        this.WhenAnyValue(x => x.SelectedParameters, bp =>
        {
            if (bp == null) return 0;
            _regions.Clear();
            _regions.AddRange(_dataProvider.LoadRegions(bp));
            return _regions.Count;
        }).ToProperty(this, x => x.NumberOfRegions, out _numberOfRegions);
    }
}