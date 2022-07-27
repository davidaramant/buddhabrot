using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using Buddhabrot.Core;
using Buddhabrot.Core.Boundary;
using Buddhabrot.Core.Images;
using ReactiveUI;

namespace BoundaryFinder.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private int _minimumIterations = 5_000_000;
    private int _maximumIterations = 15_000_000;
    private int _verticalDivisions = 1024;
    private readonly ObservableAsPropertyHelper<double> _scanAreaWidth;
    private readonly ObservableAsPropertyHelper<double> _scanArea;
    private string _output = string.Empty;
    private int _numBorderAreasFound;

    public int VerticalDistance => HorizontalDistance / 2;
    public int HorizontalDistance { get; } = 4;


    public int MinimumIterations
    {
        get => _minimumIterations;
        set => this.RaiseAndSetIfChanged(ref _minimumIterations, value);
    }

    public int MaximumIterations
    {
        get => _maximumIterations;
        set => this.RaiseAndSetIfChanged(ref _maximumIterations, value);
    }

    public int VerticalDivisions
    {
        get => _verticalDivisions;
        set => this.RaiseAndSetIfChanged(ref _verticalDivisions, value);
    }

    public int NumBorderAreasFound
    {
        get => _numBorderAreasFound;
        set => this.RaiseAndSetIfChanged(ref _numBorderAreasFound, value);
    }

    public double ScanAreaWidth => _scanAreaWidth.Value;
    public double ScanArea => _scanArea.Value;

    public ReactiveCommand<Unit, Unit> FindBoundary { get; }
    public ReactiveCommand<Unit, Unit> CancelFindingBoundary { get; }

    public string Output
    {
        get => _output;
        private set => this.RaiseAndSetIfChanged(ref _output, value);
    }

    public MainWindowViewModel()
    {
        this.WhenAnyValue(x => x.VerticalDivisions, divisions => VerticalDistance / (double)divisions)
            .ToProperty(this, x => x.ScanAreaWidth, out _scanAreaWidth);
        this.WhenAnyValue(x => x.ScanAreaWidth, width => width * width)
            .ToProperty(this, x => x.ScanArea, out _scanArea);

        FindBoundary = ReactiveCommand.CreateFromObservable(
            () => Observable
                .StartAsync(FindBoundaryAsync)
                .TakeUntil(CancelFindingBoundary!));
        CancelFindingBoundary = ReactiveCommand.Create(() => { }, FindBoundary.IsExecuting);
    }

    private async Task FindBoundaryAsync(CancellationToken cancelToken)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            NumBorderAreasFound = 0;

            var progress =
                new Progress<AreaId>(id => NumBorderAreasFound++);

            var areas = await Task.Run(
                () => BoundaryCalculator.FindBoundaryAreasAsync(
                    new PlotParameters(VerticalDivisions, new IterationRange(MinimumIterations, MaximumIterations)),
                    progress,
                    cancelToken),
                cancelToken);

            var maxBounds = areas.Aggregate<AreaId, (int X, int Y)>((0, 0),
                (max, areaId) => (Math.Max(max.X, areaId.X), Math.Max(max.Y, areaId.Y)));

            Log($"Took {stopwatch.Elapsed}");
            Log($"Max bounds: {maxBounds}");

            using var img = new RasterImage(width: maxBounds.X + 1, height: maxBounds.Y + 1);
            img.Fill(Color.White);
            foreach (var area in areas)
            {
                var y = img.Height - area.Y - 1;
                img.SetPixel(area.X, y, Color.Red);
            }

            img.Save($"vd{VerticalDivisions}.png");
        }
        catch (OperationCanceledException)
        {
            // Ignore
        }
        catch (Exception e)
        {
            Log(e.ToString());
        }
    }

    private void Log(string msg) => Dispatcher.UIThread.Post(() => Output += msg + Environment.NewLine);
}