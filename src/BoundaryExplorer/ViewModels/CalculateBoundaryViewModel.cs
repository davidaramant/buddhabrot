using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using BoundaryExplorer.Models;
using Buddhabrot.Core.Boundary;
using Buddhabrot.Core.Boundary.Classifiers;
using Buddhabrot.Core.Images;
using DynamicData.Binding;
using Humanizer;
using ReactiveUI;

namespace BoundaryExplorer.ViewModels;

public sealed class CalculateBoundaryViewModel : ViewModelBase
{
    private readonly BorderDataProvider _dataProvider;
    private readonly Action<string> _addToSystemLog;
    private int _maximumIterations = 15_000_000;
    private int _verticalDivisionPower = 1;
    private readonly ObservableAsPropertyHelper<AreaDivisions> _areaDivisions;
    private readonly ObservableAsPropertyHelper<string> _imageSize;
    private readonly ObservableAsPropertyHelper<bool> _isFindingBoundary;
    private string _log = string.Empty;
    private ClassifierType _selectedClassifier;

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
    
    public IReadOnlyCollection<ClassifierType> ClassifierTypes { get; } = Enum.GetValues<ClassifierType>();

    public ClassifierType SelectedClassifier
    {
        get => _selectedClassifier;
        set => this.RaiseAndSetIfChanged(ref _selectedClassifier, value);
    }

    public AreaDivisions AreaDivisions => _areaDivisions.Value;
    public string ImageSize => _imageSize.Value;

    public ReactiveCommand<Unit, Unit> FindBoundary { get; }
    public ReactiveCommand<Unit, Unit> CancelFindingBoundary { get; }
    public bool IsFindingBoundary => _isFindingBoundary.Value;

    public string LogOutput
    {
        get => _log;
        private set => this.RaiseAndSetIfChanged(ref _log, value);
    }

    public CalculateBoundaryViewModel(BorderDataProvider dataProvider, Action<string> addToSystemLog)
    {
        _dataProvider = dataProvider;
        _addToSystemLog = addToSystemLog;
        this.WhenAnyValue(x => x.VerticalDivisionPower, power => new AreaDivisions(power))
            .ToProperty(this, x => x.AreaDivisions, out _areaDivisions);
        this.WhenPropertyChanged(x => x.VerticalDivisionPower)
            .Select(v =>
            {
                var pixels = (2L << v.Value) * (2L << v.Value);
                var metric = ImageSizeDescription.ToMetric(pixels);
                var base2 = ImageSizeDescription.ToBase2(pixels);

                return $"{metric} ({base2})";
            })
            .ToProperty(this, x => x.ImageSize, out _imageSize);

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

            var metadata = _selectedClassifier == ClassifierType.Default
                ? string.Empty
                : _selectedClassifier.ToString();
            var boundaryParameters = new BoundaryParameters(AreaDivisions, MaximumIterations, metadata);

            var visitedRegions = new VisitedRegions(capacity: boundaryParameters.Divisions.QuadrantDivisions * 2);

            await Task.Run(
                () => BoundaryCalculator.VisitBoundary(IRegionClassifier.Create(boundaryParameters, _selectedClassifier), visitedRegions, cancelToken),
                cancelToken);

            AddToLog(DateTime.Now.ToString("s"));
            AddToLog($"Visited boundary for {boundaryParameters} ({stopwatch.Elapsed.Humanize(2)})");
            stopwatch.Restart();

            var boundaryRegions = visitedRegions.GetBoundaryRegions();

            AddToLog($"Found {boundaryRegions.Count:N0} boundary regions ({stopwatch.Elapsed.Humanize(2)})");
            stopwatch.Restart();

            var transformer = new QuadTreeTransformer(visitedRegions);
            var lookup = await Task.Run(() => transformer.Transform(), cancelToken);

            AddToLog($"Transformed quad tree to Region Lookup ({stopwatch.Elapsed.Humanize(2)})\n" +
                     $" - Went from {visitedRegions.NodeCount:N0} to {lookup.NodeCount:N0} nodes ({(double)lookup.NodeCount / visitedRegions.NodeCount:P})");

            AddToLog(string.Empty);

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
            _addToSystemLog("Error calculating boundary:" + e);
        }
    }

    private void AddToLog(string msg) => Dispatcher.UIThread.Post(() => LogOutput += msg + Environment.NewLine);
}