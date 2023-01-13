namespace Buddhabrot.Core.Boundary.Classifiers;

public static class RegionIdExtensions
{
    public static CornerId LowerLeftCorner(this RegionId region) => new(region.X, region.Y);
    public static CornerId UpperLeftCorner(this RegionId region) => new(region.X, region.Y + 1);
    public static CornerId UpperRightCorner(this RegionId region) => new(region.X + 1, region.Y + 1);
    public static CornerId LowerRightCorner(this RegionId region) => new(region.X + 1, region.Y);
}