using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Buddhabrot.Core;
using Buddhabrot.IterationKernels;
using log4net;

namespace Buddhabrot.Points
{
    sealed class VectorPointFinder : LegacyPointFinder
    {
        private static readonly ILog Log = LogManager.GetLogger(nameof(VectorPointFinder));

        private const int BatchSize = 512;

        private int VectorCapacity => Vector<float>.Count;

        public VectorPointFinder(
            RandomPointGenerator numberGenerator,
            IntRange iterationRange,
            PointWriter writer,
            PointFinderStatistics statistics) :
            base(numberGenerator, iterationRange, writer, statistics)
        {
        }

        protected override void IteratePointBatch(CancellationToken token)
        {
            var points = NumberGenerator.GetPoints(BatchSize);

            Parallel.ForEach(
                VectorBatch(points),
                (batch, loopState) =>
                {
                    if (token.IsCancellationRequested || loopState.ShouldExitCurrentIteration)
                    {
                        loopState.Stop();
                        return;
                    }

                    var realBatch = new float[VectorCapacity];
                    var imagBatch = new float[VectorCapacity];

                    for (int i = 0; i < VectorCapacity; i++)
                    {
                        realBatch[i] = batch[i].Real;
                        imagBatch[i] = batch[i].Imag;
                    }

                    var cReal = new Vector<float>(realBatch);
                    var cImag = new Vector<float>(imagBatch);

                    var vIterations = VectorKernel.IteratePoints(cReal, cImag, IterationRange.Max);

                    for (int i = 0; i < VectorCapacity; i++)
                    {
                        if (IterationRange.IsInside(vIterations[i]))
                        {
                            PointWriter.Save(batch[i]);
                        }
                    }
                });

            Statistics.AddPointCount(BatchSize);
        }

        private IEnumerable<FComplex[]> VectorBatch(IEnumerable<FComplex> pointSequence)
        {
            int count = 0;
            var batch = new FComplex[VectorCapacity];
            foreach (var complex in pointSequence)
            {
                batch[count++] = complex;

                if (count == VectorCapacity)
                {
                    count = 0;
                    yield return batch;
                    batch = new FComplex[VectorCapacity];
                }
            }
        }
    }
}
