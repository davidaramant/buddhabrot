using System.Collections.Generic;
using System.Numerics;
using Buddhabrot.Core;

namespace Buddhabrot.IterationKernels
{
    public interface IPointBatchResults
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
        Complex GetPoint(int index);

        /// <summary>
        /// Gets the iteration count for the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The iteration count.</returns>
        EscapeTime GetIteration(int index);

        /// <summary>
        /// Returns a sequence of all the results.
        /// </summary>
        /// <returns>The points and their corresponding iterations.</returns>
        IEnumerable<(Complex Point, EscapeTime Iterations)> GetAllResults();
    }
}
