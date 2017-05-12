using System.Collections.Generic;
using System.Numerics;
using System.Threading;

namespace Buddhabrot.IterationKernels
{
    /// <summary>
    /// Represents a one-time batch of work.  NOT reusable!
    /// </summary>
    public interface IPointBatch
    {
        /// <summary>
        /// The maximum amount of points that can be loaded into this batch.
        /// </summary>
        int Capacity { get; }

        /// <summary>
        /// Gets amount of points loaded into the batch.
        /// </summary>
        int Count { get; }

        void AddPoint(Complex c);
        void AddPoints(IEnumerable<Complex> c);

        /// <summary>
        /// Computes the iterations.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>
        /// The results of the batch.
        /// </returns>
        IPointBatchResults ComputeIterations(CancellationToken token);
    }
}
