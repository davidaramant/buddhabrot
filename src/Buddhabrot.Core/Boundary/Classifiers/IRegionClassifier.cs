namespace Buddhabrot.Core.Boundary.Classifiers;

public enum ClassifierType
{
	Default,
	CornerFirst,
	Internal4,
	Internal16,
}

public interface IRegionClassifier
{
	VisitedRegionType ClassifyRegion(RegionId region);
	(VisitedRegionType, string) DescribeRegion(RegionId region);

	public static IRegionClassifier Create(BoundaryParameters bp, ClassifierType type = ClassifierType.CornerFirst) =>
		type switch
		{
			ClassifierType.Default => new CornerFirstRegionClassifier(bp),

			ClassifierType.CornerFirst => new CornerFirstRegionClassifier(bp),
			ClassifierType.Internal4 => new Interior4RegionClassifier(bp),
			ClassifierType.Internal16 => new Interior16RegionClassifier(bp),
			_ => throw new ArgumentException("Unknown classifier type: " + type),
		};
}
