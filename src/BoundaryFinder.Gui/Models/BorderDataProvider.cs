using System;
using System.Collections.Generic;
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
    public SourceList<BoundaryDataSet> SavedBoundaries { get; } = new();

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

    public void SaveDiff(BoundaryParameters left, BoundaryParameters right, RegionLookup lookup)
    {
        _dataProvider.SaveDiff(left, right, lookup);

        RefreshSavedBoundaries();
    }

    private void RefreshSavedBoundaries()
    {
        SavedBoundaries.Clear();
        SavedBoundaries.AddRange(_dataProvider.GetBoundaryDataSets());
    }

    public void UpdateDataStoragePath(string newPath)
    {
        _dataProvider.DataStoragePath = newPath;
        RefreshSavedBoundaries();
    }

    public RegionLookup LoadLookup(BoundaryDataSet parameters) => _dataProvider.GetLookup(parameters);
}