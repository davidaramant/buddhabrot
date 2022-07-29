using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using BoundaryFinder.Models;
using Buddhabrot.Core.Boundary;
using Buddhabrot.Core.Images;
using DynamicData;
using ReactiveUI;

namespace BoundaryFinder.ViewModels;

public sealed class BorderDataViewModel : ViewModelBase
{
    private readonly DataSourceManager _dataSourceManager;
    private int _verticalDivisions;
    private IReadOnlyList<RegionId> _regions = Array.Empty<RegionId>();

    public ReactiveCommand<Unit, Unit> LoadDataSets { get; }
    
    public ObservableCollection<BoundaryParameters> Boundaries { get; } = new();

    public int VerticalDivisions
    {
        get => _verticalDivisions;
        private set => this.RaiseAndSetIfChanged(ref _verticalDivisions, value);
    }
    
    public BorderDataViewModel(DataSourceManager dataSourceManager)
    {
        _dataSourceManager = dataSourceManager;
        LoadDataSets = ReactiveCommand.CreateFromTask(LoadDataSetsAsync);
    }

    private async Task LoadDataSetsAsync()
    {
        Boundaries.Clear();

        var dataSets = await _dataSourceManager.DataProvider.GetBoundaryParametersAsync();
        
        Boundaries.AddRange(dataSets);
    }
    
    private void VisualizeBorder()
    {
        var maxBounds = _regions.Aggregate<RegionId, (int X, int Y)>((0, 0),
            (max, areaId) => (Math.Max(max.X, areaId.X), Math.Max(max.Y, areaId.Y)));

        using var img = new RasterImage(width: maxBounds.X + 1, height: maxBounds.Y + 1);
        img.Fill(Color.White);
        foreach (var area in _regions)
        {
            var y = img.Height - area.Y - 1;
            img.SetPixel(area.X, y, Color.Red);
        }

        img.Save($"vd{VerticalDivisions}.png");

    }

}