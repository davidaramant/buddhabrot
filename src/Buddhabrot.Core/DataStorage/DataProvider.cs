using Buddhabrot.Core.Boundary;
using Buddhabrot.Core.DataStorage.Serialization;

namespace Buddhabrot.Core.DataStorage;

public sealed class DataProvider
{
    public string DataStoragePath { get; set; } = string.Empty;

    public IReadOnlyList<BoundaryParameters> GetBoundaryParameters()
    {
        if (!Directory.Exists(DataStoragePath))
        {
            return Array.Empty<BoundaryParameters>();
        }

        return
            Directory.GetFiles(DataStoragePath, "*.boundaries")
                .Select(filePath => BoundaryParameters.FromDescription(Path.GetFileNameWithoutExtension(filePath)))
                .OrderByDescending(bp => bp.Divisions.VerticalPower)
                .ToList();
    }

    public IReadOnlyList<RegionId> GetBoundaryRegions(BoundaryParameters parameters)
    {
        using var stream = File.OpenRead(Path.Combine(DataStoragePath, ToBoundaryFileName(parameters)));
        return BoundarySerializer.LoadRegions(stream).Regions;
    }

    public RegionLookup GetLookup(BoundaryParameters parameters)
    {
        using var stream = File.OpenRead(Path.Combine(DataStoragePath, ToQuadTreeFileName(parameters)));
        return BoundarySerializer.LoadQuadTree(stream);
    }

    public void SaveBoundaryRegions(BoundaryParameters parameters, IEnumerable<RegionId> regions, RegionLookup lookup)
    {
        if (!Directory.Exists(DataStoragePath))
        {
            Directory.CreateDirectory(DataStoragePath);
        }

        using (var stream = File.Open(Path.Combine(DataStoragePath, ToBoundaryFileName(parameters)), FileMode.Create))
        {
            BoundarySerializer.Save(parameters, regions, stream);
        }
        using (var stream = File.Open(Path.Combine(DataStoragePath, ToQuadTreeFileName(parameters)), FileMode.Create))
        {
            BoundarySerializer.Save(lookup, stream);
        }
    }

    public string GetBoundaryParameterLocation(BoundaryParameters parameters)
    {
        var path = Path.Combine(DataStoragePath, parameters.Description);
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        return path;
    }

    private static string ToBoundaryFileName(BoundaryParameters parameters) =>
        parameters.Description + ".boundaries";

    private static string ToQuadTreeFileName(BoundaryParameters parameters) =>
        parameters.Description + ".quadtree";
}