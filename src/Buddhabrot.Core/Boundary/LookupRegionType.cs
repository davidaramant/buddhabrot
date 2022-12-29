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

public static class LookupRegionTypeExtensions
{
    public static bool IsBorder(this LookupRegionType type) =>
        type switch
        {
            LookupRegionType.EmptyToBorder => true,
            LookupRegionType.BorderToEmpty => true,
            LookupRegionType.FilamentToBorder => true,
            LookupRegionType.BorderToFilament => true,
            LookupRegionType.MixedDiff => true,
            _ => false
        };
}