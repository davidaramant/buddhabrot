using Buddhabrot.Core.DataStorage;

namespace BoundaryFinder.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly DataProvider _dataProvider = new();
    
    public BorderDataViewModel BorderData { get; }
    public FindNewBoundaryViewModel FindNewBoundary { get; }
    public SettingsViewModel Settings { get; }

    public MainWindowViewModel()
    {
        BorderData = new BorderDataViewModel(_dataProvider);
        FindNewBoundary = new FindNewBoundaryViewModel(_dataProvider);
        Settings = new SettingsViewModel(_dataProvider);
    }
}