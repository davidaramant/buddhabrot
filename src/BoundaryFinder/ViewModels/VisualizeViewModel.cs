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
    
    public VisualizeViewModel(BorderDataProvider dataProvider)
    {
        _dataProvider = dataProvider;
        
        this.WhenAnyValue(x => x.SelectedParameters, 
                bp => bp?.MaxIterations - 1 ?? 0)
            .ToProperty(this, x => x.MinimumIterationsCap, out _minimumIterationsCap);
    }
}