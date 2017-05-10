using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace Buddhabrot.IterationKernels
{
    public sealed class ScalarKernel : IKernel
    {
        private const int BatchSize = 256;
        private readonly Batch _pointBatch;

        public ScalarKernel()
        {
            _pointBatch = new Batch(BatchSize);
        }

        public IPointBatch GetBatch() => _pointBatch.Reset();

        private sealed class Batch : IPointBatch, IPointBatchResults
        {
            private readonly Complex[] _c;
            // There's no benefit to using longs for iterations in this kernel
            private readonly int[] _iterations;

            public int Capacity => _iterations.Length;
            public int Count { get; private set; }

            public Batch(int capacity)
            {
                _c = new Complex[capacity];
                _iterations = new int[capacity];
            }

            public Batch Reset()
            {
                Count = 0;
                return this;
            }

            public int AddPoint(Complex c)
            {
                var index = Count;
                Count++;

                _c[index] = c;
                return index;
            }

            public Complex GetPoint(int index) => _c[index];

            public long GetIteration(int index) => _iterations[index];

            public IPointBatchResults ComputeIterations(CancellationToken token, long maxIterations)
            {
                Parallel.For(
                    0,
                    Count,
                    (index, loopState) =>
                    {
                        if (token.IsCancellationRequested || loopState.ShouldExitCurrentIteration)
                        {
                            loopState.Stop();
                            return;
                        }

                        var c = _c[index];

                        var z = new Complex();
                        var z2 = new Complex();

                        int iterations = 0;
                        for (iterations = 0; iterations < maxIterations; iterations++)
                        {
                            z = z2 + c;

                            z2 = z * z;

                            if ((z2.Real + z.Imaginary) > 4)
                            {
                                break;
                            }
                        }

                        _iterations[index] = iterations;
                    });

                return this;
            }

            public IEnumerable<(Complex point, long iterations)> GetAllResults()
            {
                for (int i = 0; i < Count; i++)
                {
                    yield return (_c[i], _iterations[i]);
                }
            }
        }

        public void Dispose() { }
    }
}
