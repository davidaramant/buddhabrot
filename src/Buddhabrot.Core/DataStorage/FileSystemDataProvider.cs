using System.Text.RegularExpressions;
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
        
        return WrapList(Directory.GetFiles(_baseDirectory, "*.boundaries")
            .Select(filePath =>
            {
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                var nameRegex = new Regex(@"v(\d+)_i(\d+)", RegexOptions.Compiled);
                var match = nameRegex.Match(fileName);
                return new BoundaryParameters(
                    VerticalDivisions: int.Parse(match.Groups[1].Value),
                    MaxIterations: int.Parse(match.Groups[2].Value));
            }).ToList());
    }

    public Task<IReadOnlyList<AreaId>> GetBoundaryAreasAsync(BoundaryParameters parameters)
    {
        using var stream = File.OpenRead(Path.Combine(_baseDirectory, ToFileName(parameters)));
        return Task.FromResult(BoundarySerializer.Load(stream).AreaIds);
    }

    public Task SaveBoundaryAreasAsync(BoundaryParameters parameters, IEnumerable<AreaId> boundaryAreas)
    {
        if (!Directory.Exists(_baseDirectory))
        {
            Directory.CreateDirectory(_baseDirectory);
        }
        
        using var stream = File.Open(Path.Combine(_baseDirectory, ToFileName(parameters)), FileMode.Create);
        BoundarySerializer.Save(parameters, boundaryAreas, stream);

        return Task.CompletedTask;
    }

    private static string ToFileName(BoundaryParameters parameters) =>
        $"v{parameters.VerticalDivisions}_i{parameters.MaxIterations}.boundaries";
}