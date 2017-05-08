using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Buddhabrot.Core;

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

        public IPointBatch GetBatch() => _pointBatch;

        private sealed class PointBatch : IPointBatch, IPointBatchResults
        {
            private readonly float[] _cReals;
            private readonly float[] _cImags;
            private readonly int[] _iterations;

            public int Capacity => _iterations.Length;
            public int Count { get; private set; }

            public PointBatch(int capacity)
            {
                _cReals = new float[capacity];
                _cImags = new float[capacity];
                _iterations = new int[capacity];
            }

            public int AddPoint(FComplex c)
            {
                var index = Count++;

                _cReals[index] = c.Real;
                _cImags[index] = c.Imag;
                return index;
            }

            public FComplex GetPoint(int index) => new FComplex(
                _cReals[index],
                _cImags[index]);

            public int GetIteration(int index) => _iterations[index];

            public IPointBatchResults ComputeIterations(CancellationToken token)
            {
                var vectorCapacity = Vector<float>.Count;

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

                        var realBatch = new float[vectorCapacity];
                        var imagBatch = new float[vectorCapacity];

                        for (int i = 0; i < vectorCapacity; i++)
                        {
                            realBatch[i] = _cReals[vectorBatchIndex * vectorCapacity + i];
                            imagBatch[i] = _cImags[vectorBatchIndex * vectorCapacity + i];
                        }

                        var cReal = new Vector<float>(realBatch);
                        var cImag = new Vector<float>(imagBatch);

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

        public static Vector<int> IteratePoints(Vector<float> cReal, Vector<float> cImag, int maxIterations)
        {
            var zReal = new Vector<float>(0);
            var zImag = new Vector<float>(0);

            // Cache the squares
            // They are used to find the magnitude; reuse these values when computing the next re/im
            var zReal2 = new Vector<float>(0);
            var zImag2 = new Vector<float>(0);

            var iterations = Vector<int>.Zero;
            var increment = Vector<int>.One;

            for (int i = 0; i < maxIterations; i++)
            {
                // Doing the two multiplications with an addition is somehow faster than 2 * zReal * zImag!
                // I don't get it either
                zImag = zReal * zImag + zReal * zImag + cImag;
                zReal = zReal2 - zImag2 + cReal;

                zReal2 = zReal * zReal;
                zImag2 = zImag * zImag;

                var shouldContinue = Vector.LessThanOrEqual(zReal2 + zImag2, new Vector<float>(4));

                increment = increment & shouldContinue;
                iterations += increment;
            }

            return iterations;
        }
    }
}
