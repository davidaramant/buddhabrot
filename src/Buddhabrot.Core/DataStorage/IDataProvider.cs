using Buddhabrot.Core.Boundary;

namespace Buddhabrot.Core.DataStorage;

public interface IDataProvider
{
    Task<IReadOnlyList<BoundaryParameters>> GetBoundaryParametersAsync();

    Task<IReadOnlyList<AreaId>> GetBoundaryAreasAsync(BoundaryParameters parameters);

    Task SaveBoundaryAreasAsync(BoundaryParameters parameters, IEnumerable<AreaId> boundaryAreas);
}