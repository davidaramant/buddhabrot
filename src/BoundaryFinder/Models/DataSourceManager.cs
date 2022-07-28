using Buddhabrot.Core.DataStorage;

namespace BoundaryFinder.Models;

public sealed class DataSourceManager
{
    public IDataProvider DataProvider { get; private set; } = new NoDataProvider();

    public void SetUpFileSystemProvider(string filePath)
    {
        DataProvider = new FileSystemDataProvider(filePath);
    }
}