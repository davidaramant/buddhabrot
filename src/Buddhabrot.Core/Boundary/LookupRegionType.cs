namespace Buddhabrot.Core.Boundary;

public enum LookupRegionType
{
    Empty,
    EmptyToBorder,
    EmptyToFilament,
    BorderToEmpty,
    BorderToFilament,
    FilamentToEmpty,
    FilamentToBorder,
    MixedDiff,
}
