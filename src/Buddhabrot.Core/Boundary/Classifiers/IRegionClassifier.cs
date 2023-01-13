namespace Buddhabrot.Core.Boundary.Classifiers;

public interface IRegionClassifier
{
    VisitedRegionType ClassifyRegion(RegionId region);
}