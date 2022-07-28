using Buddhabrot.Core.Boundary;

namespace Buddhabrot.Core.DataStorage;

public sealed class NoDataProvider : IDataProvider
{
    public Task<IReadOnlyList<BoundaryParameters>> GetBoundaryParametersAsync() =>
        Task.FromResult<IReadOnlyList<BoundaryParameters>>(Array.Empty<BoundaryParameters>());

    public Task<IReadOnlyList<AreaId>> GetBoundaryAreasAsync(BoundaryParameters parameters) =>
        Task.FromResult<IReadOnlyList<AreaId>>(Array.Empty<AreaId>());

    public Task SaveBoundaryAreasAsync(BoundaryParameters parameters, IEnumerable<AreaId> boundaryAreas) =>
        Task.CompletedTask;
}