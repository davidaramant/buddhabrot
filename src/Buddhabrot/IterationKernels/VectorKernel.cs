using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace Buddhabrot.IterationKernels
{
    sealed class VectorKernel : IKernel
    {
        private const int BatchSize = 512;
        private readonly PointBatch _pointBatch;

        public VectorKernel()
        {
            _pointBatch = new PointBatch(BatchSize);
        }

        public IPointBatch GetBatch() => _pointBatch.Reset();

        private sealed class PointBatch : IPointBatch, IPointBatchResults
        {
            private readonly double[] _cReals;
            private readonly double[] _cImags;
            private readonly long[] _iterations;

            public int Capacity => _iterations.Length;
            public int Count { get; private set; }

            public PointBatch(int capacity)
            {
                _cReals = new double[capacity];
                _cImags = new double[capacity];
                _iterations = new long[capacity];
            }

            public PointBatch Reset()
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
                var vectorCapacity = Vector<double>.Count;

                var numberOfVectorBatches = Count / vectorCapacity;

                Parallel.For(
                    0,
                    numberOfVectorBatches,
                    (vectorBatchIndex, loopState) =>
                    {
                        if (token.IsCancellationRequested || loopState.ShouldExitCurrentIteration)
                        {
                            loopState.Stop();
                            return;
                        }

                        var realBatch = new double[vectorCapacity];
                        var imagBatch = new double[vectorCapacity];

                        for (int i = 0; i < vectorCapacity; i++)
                        {
                            realBatch[i] = _cReals[vectorBatchIndex * vectorCapacity + i];
                            imagBatch[i] = _cImags[vectorBatchIndex * vectorCapacity + i];
                        }

                        var cReal = new Vector<double>(realBatch);
                        var cImag = new Vector<double>(imagBatch);

                        var vIterations = IteratePoints(cReal, cImag, Constant.IterationRange.Max);

                        for (int i = 0; i < vectorCapacity; i++)
                        {
                            _iterations[vectorBatchIndex * vectorCapacity + i] = vIterations[i];
                        }
                    });

                return this;
            }
        }

        public void Dispose() { }

        public static Vector<long> IteratePoints(Vector<double> cReal, Vector<double> cImag, long maxIterations)
        {
            var zReal = new Vector<double>(0);
            var zImag = new Vector<double>(0);

            // Cache the squares
            // They are used to find the magnitude; reuse these values when computing the next re/im
            var zReal2 = new Vector<double>(0);
            var zImag2 = new Vector<double>(0);

            var iterations = Vector<long>.Zero;
            var increment = Vector<long>.One;

            for (int i = 0; i < maxIterations; i++)
            {
                // Doing the two multiplications with an addition is somehow faster than 2 * zReal * zImag!
                // I don't get it either
                zImag = zReal * zImag + zReal * zImag + cImag;
                zReal = zReal2 - zImag2 + cReal;

                zReal2 = zReal * zReal;
                zImag2 = zImag * zImag;

                var shouldContinue = Vector.LessThanOrEqual(zReal2 + zImag2, new Vector<double>(4));

                increment = increment & shouldContinue;
                iterations += increment;
            }

            return iterations;
        }
    }
}
