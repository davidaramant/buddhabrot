using Buddhabrot.Core.Boundary;
using Buddhabrot.Core.DataStorage.Serialization;

namespace Buddhabrot.Core.DataStorage;

public sealed class DataProvider
{
    public string LocalDataStoragePath { get; set; } = string.Empty;

    public IReadOnlyList<BoundaryParameters> GetBoundaryParameters()
    {
        if (!Directory.Exists(LocalDataStoragePath))
        {
            return Array.Empty<BoundaryParameters>();
        }

        return 
            Directory.GetFiles(LocalDataStoragePath, "*.boundaries")
            .Select(filePath => BoundaryParameters.FromFilePrefix(Path.GetFileNameWithoutExtension(filePath)))
            .ToList();
    }

    public IReadOnlyList<RegionId> GetBoundaryRegions(BoundaryParameters parameters)
    {
        using var stream = File.OpenRead(Path.Combine(LocalDataStoragePath, ToFileName(parameters)));
        return BoundarySerializer.Load(stream).Regions;
    }

    public void SaveBoundaryRegions(BoundaryParameters parameters, IEnumerable<RegionId> regions)
    {
        if (!Directory.Exists(LocalDataStoragePath))
        {
            Directory.CreateDirectory(LocalDataStoragePath);
        }

        using var stream = File.Open(Path.Combine(LocalDataStoragePath, ToFileName(parameters)), FileMode.Create);
        BoundarySerializer.Save(parameters, regions, stream);
    }

    public string GetBoundaryParameterLocation(BoundaryParameters parameters)
    {
        var path = Path.Combine(LocalDataStoragePath, parameters.Description);
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        return path;
    }
    
    private static string ToFileName(BoundaryParameters parameters) =>
        parameters.Description + ".boundaries";
}