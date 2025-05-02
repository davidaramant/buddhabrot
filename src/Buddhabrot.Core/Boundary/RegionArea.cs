using System.Drawing;

namespace Buddhabrot.Core.Boundary;

public readonly record struct RegionArea(Rectangle Area, LookupRegionType Type);