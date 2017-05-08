using Buddhabrot.Core;

namespace Buddhabrot.IterationKernels
{
    interface IPointBatchResults
    {
        /// <summary>
        /// Gets amount of points loaded into the batch.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Gets the point at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The point.</returns>
        FComplex GetPoint(int index);

        /// <summary>
        /// Gets the iteration count for the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The iteration count.</returns>
        int GetIteration(int index);
    }
}
