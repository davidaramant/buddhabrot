namespace Buddhabrot.Core.Boundary;

/// <summary>
/// Id of an area.
/// </summary>
/// <remarks>
/// Origin is the bottom-left (-2,0)
/// </remarks>
public record AreaId(
    int X,
    int Y);