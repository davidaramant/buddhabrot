namespace Buddhabrot.Core.Boundary;
using Region = Buddhabrot.Core.Boundary.LookupRegionType;

public static class RegionLookupDiffer
{
    private static readonly IReadOnlyDictionary<(Region Left, Region Right), Region>
        TypeMappings = new Dictionary<(Region Left, Region Right), Region>
        {
            {(Region.Empty, Region.Empty), Region.Empty},
            {(Region.Empty, Region.EmptyToBorder), Region.EmptyToBorder},
            {(Region.Empty, Region.EmptyToFilament), Region.EmptyToFilament},
            
            {(Region.EmptyToBorder, Region.Empty), Region.BorderToEmpty},
            {(Region.EmptyToBorder, Region.EmptyToBorder), Region.Empty},
            {(Region.EmptyToBorder, Region.EmptyToFilament), Region.BorderToFilament},

            {(Region.EmptyToFilament, Region.Empty), Region.Empty},
            {(Region.EmptyToFilament, Region.EmptyToBorder), Region.FilamentToBorder},
            {(Region.EmptyToFilament, Region.EmptyToFilament), Region.Empty},
        };

    public static Region DetermineType(Region left, Region right) => TypeMappings[(left, right)];
}