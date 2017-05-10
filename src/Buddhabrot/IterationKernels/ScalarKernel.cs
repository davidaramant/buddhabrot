using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Buddhabrot.Core;

namespace Buddhabrot.IterationKernels
{
    public sealed class ScalarKernel : IKernel
    {
        private const int BatchSize = 256;
        private readonly Batch _pointBatch;

        public ScalarKernel(IntRange iterationRange = null)
        {
            _pointBatch = new Batch(BatchSize, iterationRange ?? Constant.IterationRange);
        }

        public IPointBatch GetBatch() => _pointBatch.Reset();

        private sealed class Batch : IPointBatch, IPointBatchResults
        {
            private readonly IntRange _iterationRange;
            private readonly double[] _cReals;
            private readonly double[] _cImags;
            private readonly long[] _iterations;

            public int Capacity => _iterations.Length;
            public int Count { get; private set; }

            public Batch(int capacity, IntRange iterationRange)
            {
                _iterationRange = iterationRange;
                _cReals = new double[capacity];
                _cImags = new double[capacity];
                _iterations = new long[capacity];
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

                _cReals[index] = c.Real;
                _cImags[index] = c.Imaginary;
                return index;
            }

            public Complex GetPoint(int index) => new Complex(
                _cReals[index],
                _cImags[index]);

            public long GetIteration(int index) => _iterations[index];

            public IPointBatchResults ComputeIterations(CancellationToken token)
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

                        var c = new Complex(_cReals[index], _cImags[index]);

                        var z = new Complex();
                        var z2 = new Complex();

                        long iterations = 0;
                        for (iterations = 0; iterations < _iterationRange.Max; iterations++)
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
        }

        public void Dispose() { }
    }
}
