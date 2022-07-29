using Buddhabrot.Core.Boundary;

namespace Buddhabrot.Core.DataStorage;

public sealed class NoDataProvider : IDataProvider
{
    public Task<IReadOnlyList<BoundaryParameters>> GetBoundaryParametersAsync() =>
        Task.FromResult<IReadOnlyList<BoundaryParameters>>(Array.Empty<BoundaryParameters>());

    public Task<IReadOnlyList<RegionId>> GetBoundaryRegionsAsync(BoundaryParameters parameters) =>
        Task.FromResult<IReadOnlyList<RegionId>>(Array.Empty<RegionId>());

    public Task SaveBoundaryRegionsAsync(BoundaryParameters parameters, IEnumerable<RegionId> regions) =>
        Task.CompletedTask;
}