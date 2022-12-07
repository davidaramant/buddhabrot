using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Buddhabrot.Core.Boundary;
using Buddhabrot.Core.DataStorage;
using DynamicData;

namespace BoundaryFinder.Gui.Models;

public sealed class BorderDataProvider
{
    private readonly string _defaultDataSetPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), 
        "Buddhabrot",
        "Mandelbrot Set Boundaries");

    private readonly DataProvider _dataProvider;
    public string LocalDataStoragePath => _dataProvider.DataStoragePath;
    public ObservableCollection<BoundaryParameters> SavedBoundaries { get; } = new();

    public BorderDataProvider(DataProvider dataProvider)
    {
        _dataProvider = dataProvider;

        if (string.IsNullOrWhiteSpace(_dataProvider.DataStoragePath))
        {
            _dataProvider.DataStoragePath = _defaultDataSetPath;
        }

        RefreshSavedBoundaries();
    }

    public void SaveBorderData(BoundaryParameters parameters, IEnumerable<RegionId> regions, RegionLookup lookup)
    {
        _dataProvider.SaveBoundaryRegions(parameters, regions, lookup);

        RefreshSavedBoundaries();
    }

    private void RefreshSavedBoundaries()
    {
        SavedBoundaries.Clear();
        SavedBoundaries.AddRange(_dataProvider.GetBoundaryParameters());
    }

    public void UpdateDataStoragePath(string newPath)
    {
        _dataProvider.DataStoragePath = newPath;
        RefreshSavedBoundaries();
    }

    public IReadOnlyList<RegionId> LoadRegions(BoundaryParameters parameters) =>
        _dataProvider.GetBoundaryRegions(parameters);

    public RegionLookup LoadLookup(BoundaryParameters parameters) => _dataProvider.GetLookup(parameters);
}