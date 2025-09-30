namespace Buddhabrot.Core.Boundary.Classifiers;

public enum ClassifierType
{
	CornerFirstFourInternal,
	CornerFirstOneInternal,
	Internal4,
	Internal16,
	Default = CornerFirstOneInternal,
}

public interface IRegionClassifier
{
	VisitedRegionType ClassifyRegion(RegionId region);
	(VisitedRegionType, string) DescribeRegion(RegionId region);

	public static IRegionClassifier Create(BoundaryParameters bp, ClassifierType type = ClassifierType.Default) =>
		type switch
		{
			ClassifierType.CornerFirstFourInternal => new CornerFirstFourInternalRegionClassifier(bp),
			ClassifierType.CornerFirstOneInternal => new CornerFirstOneInternalRegionClassifier(bp),
			ClassifierType.Internal4 => new Interior4RegionClassifier(bp),
			ClassifierType.Internal16 => new Interior16RegionClassifier(bp),
			_ => throw new ArgumentException("Unknown classifier type: " + type),
		};
}
