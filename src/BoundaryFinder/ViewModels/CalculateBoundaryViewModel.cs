using System;
using System.Diagnostics;
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
    private readonly ObservableAsPropertyHelper<int> _verticalDivisions;
    private readonly ObservableAsPropertyHelper<double> _scanAreaWidth;
    private readonly ObservableAsPropertyHelper<double> _scanArea;
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

    public int VerticalDivisions => _verticalDivisions.Value;

    public double ScanAreaWidth => _scanAreaWidth.Value;
    public double ScanArea => _scanArea.Value;

    public ReactiveCommand<Unit, Unit> FindBoundary { get; }
    public ReactiveCommand<Unit, Unit> CancelFindingBoundary { get; }
    public bool IsFindingBoundary => _isFindingBoundary.Value;
    
    public CalculateBoundaryViewModel(BorderDataProvider dataProvider, Action<string> log)
    {
        _dataProvider = dataProvider;
        _log = log;
        this.WhenAnyValue(x => x.VerticalDivisionPower, power => 1 << power)
            .ToProperty(this, x => x.VerticalDivisions, out _verticalDivisions);
        this.WhenAnyValue(x => x.VerticalDivisions, divisions => VerticalDistance / (double)divisions)
            .ToProperty(this, x => x.ScanAreaWidth, out _scanAreaWidth);
        this.WhenAnyValue(x => x.ScanAreaWidth, width => width * width)
            .ToProperty(this, x => x.ScanArea, out _scanArea);

        FindBoundary = ReactiveCommand.CreateFromObservable(
            () => Observable
                .StartAsync(FindBoundaryAsync)
                .TakeUntil(CancelFindingBoundary!));
        FindBoundary.IsExecuting.ToProperty(this,x=>x.IsFindingBoundary, out _isFindingBoundary);
        CancelFindingBoundary = ReactiveCommand.Create(() => { }, FindBoundary.IsExecuting);
    }

    private async Task FindBoundaryAsync(CancellationToken cancelToken)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();

            var boundaryParameters = new BoundaryParameters(VerticalDivisions, MaximumIterations);
            
            var regions = await Task.Run(
                () => BoundaryCalculator.FindBoundaryAsync(
                    boundaryParameters,
                    cancelToken),
                cancelToken);

            _log($"Found boundary for {boundaryParameters}. Took {stopwatch.Elapsed}, Found {regions.Count:N0} border regions");

            _dataProvider.SaveBorderData(boundaryParameters, regions);
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