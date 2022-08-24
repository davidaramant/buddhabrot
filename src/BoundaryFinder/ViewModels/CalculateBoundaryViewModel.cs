using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using BoundaryFinder.Models;
using Buddhabrot.Core.Boundary;
using ReactiveUI;

namespace BoundaryFinder.ViewModels;

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

            var regions = await Task.Run(
                () => BoundaryCalculator.FindBoundaryAndFilamentsAsync(
                    boundaryParameters,
                    cancelToken),
                cancelToken);

            _log(
                $"Found boundary for {boundaryParameters}.\n\t- Took {stopwatch.Elapsed}\n\t- Found {regions.Count:N0} border regions");
            stopwatch.Restart();

            var lookup =
                await Task.Run(() => new RegionLookup(boundaryParameters.Divisions.VerticalPower, regions, _log),
                    cancelToken);

            _log($"Constructed quad tree. Took {stopwatch.Elapsed}");

            _dataProvider.SaveBorderData(
                boundaryParameters,
                regions.Where(pair => pair.Type == RegionType.Border).Select(pair => pair.Region).ToList(),
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