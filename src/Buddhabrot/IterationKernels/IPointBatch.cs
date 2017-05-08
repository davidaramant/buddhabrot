using System.Threading;
using Buddhabrot.Core;

namespace Buddhabrot.IterationKernels
{
    interface IPointBatch
    {
        /// <summary>
        /// The maximum amount of points that can be loaded into this batch.
        /// </summary>
        int Capacity { get; }

        /// <summary>
        /// Gets amount of points loaded into the batch.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Adds a point to the batch
        /// </summary>
        /// <param name="c">The point.</param>
        /// <returns>
        /// The index it was added to.
        /// </returns>
        int AddPoint(FComplex c);

        /// <summary>
        /// Computes the iterations.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>The results of the batch.</returns>
        IPointBatchResults ComputeIterations(CancellationToken token);
    }
}
