using BoundaryFinder.Models;

namespace BoundaryFinder.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly DataSourceManager _dataSourceManager = new();
    
    public DataSourceViewModel DataSource { get; }
    
    public BorderDataViewModel BorderData { get; }
    public FindNewBoundaryViewModel FindNewBoundary { get; }

    public MainWindowViewModel()
    {
        DataSource = new DataSourceViewModel(_dataSourceManager);
        BorderData = new BorderDataViewModel(_dataSourceManager);
        FindNewBoundary = new FindNewBoundaryViewModel(_dataSourceManager);
    }
}