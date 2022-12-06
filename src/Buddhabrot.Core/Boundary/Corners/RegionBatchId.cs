﻿using Buddhabrot.Core.Utilities;

namespace Buddhabrot.Core.Boundary.Corners;

public readonly record struct RegionBatchId(int X, int Y)
{
    public static readonly RegionBatchId Invalid = new(int.MaxValue, int.MaxValue);

    public CornerId GetBottomLeftCorner() => new(X * 4, Y * 4);
    public RegionId GetBottomLeftRegion() => new(X * 2, Y * 2);

    public int GetHashCode16() => XYHash.GetHashCode16(X, Y);
    public int GetHashCode64() => XYHash.GetHashCode64(X, Y);
}

public static class RegionBatchExtensions
{
    public static RegionBatchId ToBatchId(this CornerId id) => new(id.X / 4, id.Y / 4);

    public static int GetBatchIndex(this CornerId id) => ((id.Y % 4) << 2) + (id.X % 4);


    public static RegionBatchId ToBatchId(this RegionId id) => new(id.X / 2, id.Y / 2);

    public static int GetBatchIndex(this RegionId id) => ((id.Y % 2) << 1) + (id.X % 2);
}