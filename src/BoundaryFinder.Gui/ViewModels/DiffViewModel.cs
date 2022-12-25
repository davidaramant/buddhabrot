using System;
using BoundaryFinder.Gui.Models;

namespace BoundaryFinder.Gui.ViewModels;

public sealed class DiffViewModel : ViewModelBase
{
    private readonly BorderDataProvider _dataProvider;
    private readonly Action<string> _log;

    public DiffViewModel(BorderDataProvider dataProvider, Action<string> log)
    {
        _dataProvider = dataProvider;
        _log = log;
    }
}