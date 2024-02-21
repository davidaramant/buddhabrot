namespace Buddhabrot.Core.Boundary.QuadTrees;

/// <summary>
/// The different quadrants of a quad
/// </summary>
/// <remarks>
/// The order of these elements is vital for <see cref="QuadNode"/> and <see cref="QuadDimensions"/>
/// </remarks>
public enum Quadrant
{
    /// <summary>
    /// Southwest / Lower left
    /// </summary>
    SW = 0,

    /// <summary>
    /// Southeast / Lower right
    /// </summary>
    SE = 1,

    /// <summary>
    /// Northwest / Upper left
    /// </summary>
    NW = 2,

    /// <summary>
    /// Northeast / Upper right
    /// </summary>
    NE = 3,
}
