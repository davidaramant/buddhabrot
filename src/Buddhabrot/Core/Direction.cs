namespace Buddhabrot.Core
{
    public enum Direction : byte
    {
        /// <summary>
        /// Increasing imaginary
        /// </summary>
        Up = 0,
        /// <summary>
        /// Increasing real, Increasing imaginary
        /// </summary>
        UpRight = 1,
        /// <summary>
        /// Increasing real
        /// </summary>
        Right = 2,
        /// <summary>
        /// Increasing real, Decreasing imaginary
        /// </summary>
        DownRight = 3,
        /// <summary>
        /// Decreasing imaginary
        /// </summary>
        Down = 4,
        /// <summary>
        /// Decreasing real, Decreasing imaginary
        /// </summary>
        DownLeft = 5,
        /// <summary>
        /// Decreasing real
        /// </summary>
        Left = 6,
        /// <summary>
        /// Decreasing real, Increasing imaginary
        /// </summary>
        UpLeft = 7,
    }
}
