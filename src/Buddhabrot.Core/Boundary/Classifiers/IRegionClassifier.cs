namespace Buddhabrot.Core.Boundary.Classifiers;

public enum ClassifierType
{
    CornerFirst,
    Internal16,
}

public interface IRegionClassifier
{
    VisitedRegionType ClassifyRegion(RegionId region);

    public static IRegionClassifier Create(BoundaryParameters bp, ClassifierType type = ClassifierType.CornerFirst) =>
        type switch
        {
            ClassifierType.CornerFirst => new CornerFirstRegionClassifier(bp),
            ClassifierType.Internal16 => new Interior16RegionClassifier(bp),
            _ => throw new ArgumentException("Unknown classifier type: " + type)
        };
}