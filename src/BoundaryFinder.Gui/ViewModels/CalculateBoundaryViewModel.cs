using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using BoundaryFinder.Gui.Models;
using Buddhabrot.Core.Boundary;
using Buddhabrot.Core.Utilities;
using Humanizer;
using ReactiveUI;

namespace BoundaryFinder.Gui.ViewModels;

public sealed class CalculateBoundaryViewModel : ViewModelBase
{
    private readonly BorderDataProvider _dataProvider;
    private readonly Action<string> _log;
    private int _maximumIterations = 15_000_000;
    private int _verticalDivisionPower = 1;
    private readonly ObservableAsPropertyHelper<AreaDivisions> _areaDivisions;
    private readonly ObservableAsPropertyHelper<bool> _isFindingBoundary;

    public int VerticalDistance => HorizontalDistance / 2;
    public int HorizontalDistance { get; } = 4;

    public int MaximumIterations
    {
        get => _maximumIterations;
        set => this.RaiseAndSetIfChanged(ref _maximumIterations, value);
    }

    public int VerticalDivisionPower
    {
        get => _verticalDivisionPower;
        set => this.RaiseAndSetIfChanged(ref _verticalDivisionPower, value);
    }

    public AreaDivisions AreaDivisions => _areaDivisions.Value;

    public ReactiveCommand<Unit, Unit> FindBoundary { get; }
    public ReactiveCommand<Unit, Unit> CancelFindingBoundary { get; }
    public bool IsFindingBoundary => _isFindingBoundary.Value;

    public CalculateBoundaryViewModel(BorderDataProvider dataProvider, Action<string> log)
    {
        _dataProvider = dataProvider;
        _log = log;
        this.WhenAnyValue(x => x.VerticalDivisionPower, power => new AreaDivisions(power))
            .ToProperty(this, x => x.AreaDivisions, out _areaDivisions);

        FindBoundary = ReactiveCommand.CreateFromObservable(
            () => Observable
                .StartAsync(FindBoundaryAsync)
                .TakeUntil(CancelFindingBoundary!));
        FindBoundary.IsExecuting.ToProperty(this, x => x.IsFindingBoundary, out _isFindingBoundary);
        CancelFindingBoundary = ReactiveCommand.Create(() => { }, FindBoundary.IsExecuting);
    }

    private async Task FindBoundaryAsync(CancellationToken cancelToken)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();

            var boundaryParameters = new BoundaryParameters(AreaDivisions, MaximumIterations);

            var visitedRegions = new VisitedRegions(capacity: boundaryParameters.Divisions.QuadrantDivisions * 2);

            await Task.Run(
                () => BoundaryCalculator.VisitBoundary(boundaryParameters, visitedRegions, cancelToken),
                cancelToken);

            _log(DateTime.Now.ToString("s"));
            _log(ComputerDescription.GetSingleLine());
            _log($"Visited boundary for {boundaryParameters} ({stopwatch.Elapsed.Humanize(2)})");
            stopwatch.Restart();

            var boundaryRegions = visitedRegions.GetBoundaryRegions();

            _log($"Found {boundaryRegions.Count:N0} boundary regions ({stopwatch.Elapsed.Humanize(2)})");
            stopwatch.Restart();

            var transformer = new VisitedRegionsToRegionLookup(visitedRegions);
            var lookup = await Task.Run(() => transformer.Transform(), cancelToken);
            
            _log($"Normalized quad tree to Region Lookup ({stopwatch.Elapsed.Humanize(2)})\n"+
                 $" - Went from {visitedRegions.NodeCount:N0} to {lookup.NodeCount:N0} nodes ({(double)lookup.NodeCount/visitedRegions.NodeCount:P})");

            _log(string.Empty);

            _dataProvider.SaveBorderData(
                boundaryParameters,
                boundaryRegions,
                lookup);
        }
        catch (OperationCanceledException)
        {
            // Ignore
        }
        catch (Exception e)
        {
            _log(e.ToString());
        }
    }
}