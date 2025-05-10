using SkiaSharp;

namespace Buddhabrot.Core.Boundary;

public readonly record struct RegionArea(SKRectI Area, LookupRegionType Type);
