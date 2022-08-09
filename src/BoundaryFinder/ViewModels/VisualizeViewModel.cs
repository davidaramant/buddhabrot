using System.Collections.ObjectModel;
using BoundaryFinder.Models;
using Buddhabrot.Core.Boundary;

namespace BoundaryFinder.ViewModels;

public sealed class VisualizeViewModel : ViewModelBase
{
    private readonly BorderDataProvider _dataProvider;

    public ObservableCollection<BoundaryParameters> SavedBoundaries => _dataProvider.SavedBoundaries;

    public VisualizeViewModel(BorderDataProvider dataProvider)
    {
        _dataProvider = dataProvider;
        
        // TODO: Expose MaximumIterations; bind the MinimumIterations spinner Max to it
    }
}