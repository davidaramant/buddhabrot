using BoundaryFinder.Models;

namespace BoundaryFinder.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly DataSourceManager _dataSourceManager = new();
    
    public ConfigureDataSourceViewModel ConfigureDataSource { get; }
    
    public BorderDataViewModel BorderData { get; }
    public FindNewBoundaryViewModel FindNewBoundary { get; }

    public MainWindowViewModel()
    {
        ConfigureDataSource = new ConfigureDataSourceViewModel(_dataSourceManager);
        BorderData = new BorderDataViewModel(_dataSourceManager);
        FindNewBoundary = new FindNewBoundaryViewModel(_dataSourceManager);
    }
}