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
    /// Lower left
    /// </summary>
    LL = 0,
    /// <summary>
    /// Lower right
    /// </summary>
    LR = 1,
    /// <summary>
    /// Upper left
    /// </summary>
    UL = 2,
    /// <summary>
    /// Upper right
    /// </summary>
    UR = 3,
}