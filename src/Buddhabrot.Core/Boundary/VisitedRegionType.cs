namespace Buddhabrot.Core.Boundary;

public enum VisitedRegionType
{
    Unknown,
    Border,
    Filament,
    Rejected,
    
    /// <summary>
    /// A region that includes part of the set but fails heuristic checks. 
    /// </summary>
    /// <remarks>
    /// Used in the boundary calculator algorithm but not stored - it gets converted to Rejected.
    /// </remarks>
    Mediocre
}