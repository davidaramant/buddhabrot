using Buddhabrot.Core.Utilities;

namespace Buddhabrot.Core.Boundary.Corners;

public readonly record struct RegionBatchId(int X, int Y)
{
    public const int CornerPower = 2;
    public const int CornerWidth = 1 << CornerPower;
    public const int CornerArea = CornerWidth * CornerWidth;
    public const int RegionPower = 1;
    public const int RegionWidth = 1 << RegionPower;
    public const int RegionArea = RegionWidth * RegionWidth;

    public static readonly RegionBatchId Invalid = new(int.MaxValue, int.MaxValue);

    public CornerId GetBottomLeftCorner() => new(X * CornerWidth, Y * CornerWidth);
    public RegionId GetBottomLeftRegion() => new(X * RegionWidth, Y * RegionWidth);

    public int GetHashCode16() => XYHash.GetHashCode16(X, Y);
    public int GetHashCode64() => XYHash.GetHashCode64(X, Y);
}
