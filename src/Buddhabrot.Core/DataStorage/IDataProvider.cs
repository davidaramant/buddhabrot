using Buddhabrot.Core.Boundary;

namespace Buddhabrot.Core.DataStorage;

public interface IDataProvider
{
    Task<IReadOnlyList<BoundaryParameters>> GetBoundaryParametersAsync();

    Task<IReadOnlyList<RegionId>> GetBoundaryRegionsAsync(BoundaryParameters parameters);

    Task SaveBoundaryRegionsAsync(BoundaryParameters parameters, IEnumerable<RegionId> regions);
}