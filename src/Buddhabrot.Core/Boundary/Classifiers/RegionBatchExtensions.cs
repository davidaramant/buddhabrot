using static Buddhabrot.Core.Boundary.Classifiers.RegionBatchId;

namespace Buddhabrot.Core.Boundary.Classifiers;

public static class RegionBatchExtensions
{
	public static RegionBatchId ToBatchId(this CornerId id) => new(id.X / CornerWidth, id.Y / CornerWidth);

	public static int GetBatchIndex(this CornerId id) => ((id.Y % CornerWidth) << CornerPower) + (id.X % CornerWidth);

	public static RegionBatchId ToBatchId(this RegionId id) => new(id.X / RegionWidth, id.Y / RegionWidth);

	public static int GetBatchIndex(this RegionId id) => ((id.Y % RegionWidth) << RegionPower) + (id.X % RegionWidth);
}
