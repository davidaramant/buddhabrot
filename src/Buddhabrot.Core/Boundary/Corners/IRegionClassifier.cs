namespace Buddhabrot.Core.Boundary.Corners;

public interface IRegionClassifier
{
    VisitedRegionType ClassifyRegion(RegionId region);
}