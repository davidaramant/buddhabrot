using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using BoundaryFinder.Models;
using Buddhabrot.Core.Boundary;
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
    private RegionLookup _lookup = RegionLookup.Empty;
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

    public RegionLookup Lookup
    {
        get => _lookup;
        private set => this.RaiseAndSetIfChanged(ref _lookup, value);
    }

    public VisualizeViewModel(BorderDataProvider dataProvider, Action<string> log)
    {
        _dataProvider = dataProvider;
        _log = log;

        var loadLookupCommand = ReactiveCommand.Create<BoundaryParameters>(LoadLookup);

        this.WhenAnyValue(x => x.SelectedParameters,
                bp => bp?.MaxIterations - 1 ?? 0)
            .ToProperty(this, x => x.MinimumIterationsCap, out _minimumIterationsCap);

        this.WhenPropertyChanged(x => x.SelectedParameters, notifyOnInitialValue: false)
            .Where(x => x.Value != null)
            .Select(x => x.Value!)
            .InvokeCommand(loadLookupCommand);
    }

    private void LoadLookup(BoundaryParameters parameters)
    {
        try
        {
            IsLoadingBoundary = true;
            var lookup = _dataProvider.LoadLookup(SelectedParameters);
            
            Lookup = lookup;
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