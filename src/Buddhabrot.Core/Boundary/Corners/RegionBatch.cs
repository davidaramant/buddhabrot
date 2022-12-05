using System.Diagnostics.Metrics;

namespace Buddhabrot.Core.Boundary.Corners;

public readonly struct RegionBatch
{
    private readonly uint _data;

    private RegionBatch(uint data) => _data = data;

    public RegionBatch WithCornerInSet(int index) => new(_data | (uint)(1 << index));
    public RegionBatch WithCenterInFilament(int index) => new(_data | (uint)(1 << (index + 16)));

    public bool IsCornerInSet(CornerId corner) => (_data & (1 << corner.GetBatchIndex())) != 0;
}
