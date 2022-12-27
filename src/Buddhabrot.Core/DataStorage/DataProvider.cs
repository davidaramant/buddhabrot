using Buddhabrot.Core.Boundary;
using Buddhabrot.Core.DataStorage.Serialization;

namespace Buddhabrot.Core.DataStorage;

public sealed class DataProvider
{
    public string DataStoragePath { get; set; } = string.Empty;

    public IReadOnlyList<BoundaryDataSet> GetRegionLookupDataSets()
    {
        if (!Directory.Exists(DataStoragePath))
        {
            return Array.Empty<BoundaryDataSet>();
        }

        return
            Directory.GetFiles(DataStoragePath, "*.quadtree")
                .Select(filePath => BoundaryDataSet.FromDescription(Path.GetFileNameWithoutExtension(filePath)))
                .OrderBy(bp => bp)
                .ToList();
    }

    public RegionLookup GetLookup(BoundaryDataSet parameters)
    {
        using var stream = File.OpenRead(Path.Combine(DataStoragePath, ToQuadTreeFileName(parameters)));
        return BoundarySerializer.LoadQuadTree(stream);
    }

    public IReadOnlyList<RegionId> GetBoundaryRegions(BoundaryParameters parameters)
    {
        using var stream = File.OpenRead(Path.Combine(DataStoragePath, ToBoundaryFileName(parameters)));
        return BoundarySerializer.LoadRegions(stream).Regions;
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

    public void SaveDiff(BoundaryParameters left, BoundaryParameters right, RegionLookup lookup)
    {
        // Don't bother checking directory, it must exist if we're generating a diff

        using var stream =
            File.Open(Path.Combine(DataStoragePath, ToQuadTreeFileName(BoundaryDataSet.FromDiff(left, right))),
                FileMode.Create);
        BoundarySerializer.Save(lookup, stream);
    }


    private static string ToBoundaryFileName(BoundaryParameters parameters) =>
        parameters.Description + ".boundaries";

    private static string ToQuadTreeFileName(BoundaryParameters parameters) =>
        parameters.Description + ".quadtree";

    private static string ToQuadTreeFileName(BoundaryDataSet parameters) =>
        parameters.Description + ".quadtree";
}