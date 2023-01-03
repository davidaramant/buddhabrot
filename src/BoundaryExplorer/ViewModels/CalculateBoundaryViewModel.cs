using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using BoundaryExplorer.Models;
using Buddhabrot.Core.Boundary;
using Buddhabrot.Core.Utilities;
using DynamicData.Binding;
using Humanizer;
using ReactiveUI;

namespace BoundaryExplorer.ViewModels;

public sealed class CalculateBoundaryViewModel : ViewModelBase
{
    private readonly BorderDataProvider _dataProvider;
    private readonly Action<string> _log;
    private int _maximumIterations = 15_000_000;
    private int _verticalDivisionPower = 1;
    private readonly ObservableAsPropertyHelper<AreaDivisions> _areaDivisions;
    private readonly ObservableAsPropertyHelper<string> _imageSize;
    private readonly ObservableAsPropertyHelper<bool> _isFindingBoundary;

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
    public string ImageSize => _imageSize.Value;

    public ReactiveCommand<Unit, Unit> FindBoundary { get; }
    public ReactiveCommand<Unit, Unit> CancelFindingBoundary { get; }
    public bool IsFindingBoundary => _isFindingBoundary.Value;

    public CalculateBoundaryViewModel(BorderDataProvider dataProvider, Action<string> log)
    {
        _dataProvider = dataProvider;
        _log = log;
        this.WhenAnyValue(x => x.VerticalDivisionPower, power => new AreaDivisions(power))
            .ToProperty(this, x => x.AreaDivisions, out _areaDivisions);
        this.WhenPropertyChanged(x => x.VerticalDivisionPower)
            .Select(v =>
            {
                var pixels = (2L << v.Value) * (2L << v.Value);
                var metric = ToMetricPixelSize(pixels);
                var base2 = ToBase2PixelSize(pixels);

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

    [SuppressMessage("ReSharper", "IdentifierTypo")]
    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    private static string ToBase2PixelSize(long pixels)
    {
        const long kibipixel = 1_024;
        const long mebipixel = kibipixel * kibipixel;
        const long gibipixel = kibipixel * mebipixel;
        const long tebipixel = kibipixel * gibipixel;

        return pixels switch
        {
            < kibipixel => $"{pixels} pixels",
            < mebipixel => $"{pixels/kibipixel} kibipixels",
            < gibipixel => $"{pixels/mebipixel} mebipixels",
            < tebipixel => $"{pixels/gibipixel} gibipixels",
            _ => $"{pixels/tebipixel:N0} tebipixels",
        };
    }
    
    [SuppressMessage("ReSharper", "IdentifierTypo")]
    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    private static string ToMetricPixelSize(long pixels)
    {
        const long kilopixel = 1_000;
        const long megapixel = kilopixel * kilopixel;
        const long gigapixel = kilopixel * megapixel;
        const long terapixel = kilopixel * gigapixel;

        return pixels switch
        {
            < kilopixel => $"{pixels} pixels",
            < megapixel => $"{(double)pixels/kilopixel:N1} kilopixels",
            < gigapixel => $"{(double)pixels/megapixel:N1} megapixels",
            < terapixel => $"{(double)pixels/gigapixel:N1} gigapixels",
            _ => $"{(double)pixels/terapixel:N1} terapixels",
        };
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

            var transformer = new QuadTreeTransformer(visitedRegions);
            var lookup = await Task.Run(() => transformer.Transform(), cancelToken);

            _log($"Transformed quad tree to Region Lookup ({stopwatch.Elapsed.Humanize(2)})\n" +
                 $" - Went from {visitedRegions.NodeCount:N0} to {lookup.NodeCount:N0} nodes ({(double) lookup.NodeCount / visitedRegions.NodeCount:P})");

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