using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Buddhabrot.Core;

namespace Buddhabrot.IterationKernels
{
    public sealed class ScalarFloatKernel : IKernel
    {
        public KernelType Type => KernelType.ScalarFloat;
        private const int BatchSize = 256;
        private readonly Batch _pointBatch;

        public ScalarFloatKernel()
        {
            _pointBatch = new Batch(BatchSize);
        }

        public IPointBatch GetBatch() => _pointBatch.Reset();

        private sealed class Batch : IPointBatch, IPointBatchResults
        {
            private readonly Complex[] _c;
            private readonly EscapeTime[] _iterations;

            public int Capacity => _iterations.Length;
            public int Count { get; private set; }

            public Batch(int capacity)
            {
                _c = new Complex[capacity];
                _iterations = new EscapeTime[capacity];
            }

            public Batch Reset()
            {
                Count = 0;
                return this;
            }

            public void AddPoint(Complex c)
            {
                var index = Count;
                Count++;

                _c[index] = c;
            }

            public void AddPoints(IEnumerable<Complex> points)
            {
                int index = Count;
                foreach (var point in points)
                {
                    _c[index] = point;
                    Count++;
                }
            }

            public Complex GetPoint(int index) => _c[index];

            public EscapeTime GetIteration(int index) => _iterations[index];

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

                        _iterations[index] = FindEscapeTime(_c[index]);
                    });

                return this;
            }

            /// <summary>
            /// Determines whether the point is in the set, without using an arbitrary iteration limit.
            /// </summary>
            /// <param name="c">The c.</param>
            /// <remarks>
            /// Brent's Algorithm is used to detect cycles for points in the set.
            /// </remarks>
            /// <returns>
            /// The <see cref="EscapeTime"/> of the point.
            /// </returns>
            private static EscapeTime FindEscapeTime(Complex c)
            {
                if (MandelbulbChecker.IsInsideBulbs(c))
                    return EscapeTime.Infinite;

                var zReal = 0.0f;
                var zImag = 0.0f;

                var z2Real = 0.0f;
                var z2Imag = 0.0f;

                var oldZReal = 0.0f;
                var oldZImag = 0.0f;

                var cReal = (float)c.Real;
                var cImag = (float)c.Imaginary;

                int stepsTaken = 0;
                int stepLimit = 2;

                int iterations = 0;
                while ((z2Real + z2Imag) <= 4)
                {
                    iterations++;
                    stepsTaken++;

                    zImag = 2 * zReal * zImag + cImag;
                    zReal = z2Real - z2Imag + cReal;

                    z2Real = zReal * zReal;
                    z2Imag = zImag * zImag;

                    if (oldZReal == zReal && oldZImag == zImag)
                        return EscapeTime.Infinite;

                    if (stepsTaken == stepLimit)
                    {
                        oldZReal = zReal;
                        oldZImag = zImag;
                        stepsTaken = 0;
                        stepLimit = stepLimit << 1;
                    }
                }

                return EscapeTime.Discrete(iterations);
            }

            public IEnumerable<(Complex Point, EscapeTime Iterations)> GetAllResults()
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
