using Buddhabrot.Core.Boundary;
using Buddhabrot.Core.DataStorage.Serialization;

namespace Buddhabrot.Core.DataStorage;

public sealed class FileSystemDataProvider : IDataProvider
{
    private readonly string _baseDirectory;

    public FileSystemDataProvider(string baseDirectory)
    {
        _baseDirectory = baseDirectory;
    }

    public Task<IReadOnlyList<BoundaryParameters>> GetBoundaryParametersAsync()
    {
        Task<IReadOnlyList<BoundaryParameters>> WrapList(IReadOnlyList<BoundaryParameters> parameters) =>
            Task.FromResult(parameters);

        if (!Directory.Exists(_baseDirectory))
        {
            return WrapList(Array.Empty<BoundaryParameters>());
        }

        return WrapList(
            Directory.GetFiles(_baseDirectory, "*.boundaries")
            .Select(filePath => BoundaryParameters.FromFilePrefix(Path.GetFileNameWithoutExtension(filePath)))
            .ToList());
    }

    public Task<IReadOnlyList<RegionId>> GetBoundaryRegionsAsync(BoundaryParameters parameters)
    {
        using var stream = File.OpenRead(Path.Combine(_baseDirectory, ToFileName(parameters)));
        return Task.FromResult(BoundarySerializer.Load(stream).Regions);
    }

    public Task SaveBoundaryRegionsAsync(BoundaryParameters parameters, IEnumerable<RegionId> regions)
    {
        if (!Directory.Exists(_baseDirectory))
        {
            Directory.CreateDirectory(_baseDirectory);
        }

        using var stream = File.Open(Path.Combine(_baseDirectory, ToFileName(parameters)), FileMode.Create);
        BoundarySerializer.Save(parameters, regions, stream);

        return Task.CompletedTask;
    }

    private static string ToFileName(BoundaryParameters parameters) =>
        parameters.ToFilePrefix() + ".boundaries";
}