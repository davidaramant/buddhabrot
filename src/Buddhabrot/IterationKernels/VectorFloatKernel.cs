using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Buddhabrot.Core;
using Humanizer;

namespace Buddhabrot.IterationKernels
{
    public sealed class VectorFloatKernel : IKernel
    {
        public KernelType Type => KernelType.VectorFloat;
        public static int VectorCapacity => Vector<float>.Count;

        private const int BatchSize = 512;
        private readonly PointBatch _pointBatch;

        public VectorFloatKernel()
        {
            _pointBatch = new PointBatch(BatchSize);
        }

        public IPointBatch GetBatch() => _pointBatch.Reset();

        private sealed class PointBatch : IPointBatch, IPointBatchResults
        {
            private readonly Complex[] _c;
            private readonly EscapeTime[] _iterations;

            public int Capacity => _iterations.Length;
            public int Count { get; private set; }

            public PointBatch(int capacity)
            {
                _c = new Complex[capacity];
                _iterations = new EscapeTime[capacity];
            }

            public PointBatch Reset()
            {
                Count = 0;
                return this;
            }

            public void AddPoint(Complex c)
            {
                _c[Count++] = c;
            }

            public void AddPoints(IEnumerable<Complex> points)
            {
                foreach (var point in points)
                {
                    _c[Count] = point;
                    Count++;
                }
            }

            public Complex GetPoint(int index) => _c[index];

            public EscapeTime GetIteration(int index) => _iterations[index];

            public IPointBatchResults ComputeIterations(CancellationToken token)
            {
                var numberOfVectorBatches = Count / VectorCapacity;
                var lastBatchSize = Count % VectorCapacity;

                if (lastBatchSize != 0)
                {
                    numberOfVectorBatches++;
                }
                else
                {
                    lastBatchSize = VectorCapacity;
                }

                Console.Out.WriteLine($"Count: {Count}");
                Console.Out.WriteLine($"Vector Batches: {numberOfVectorBatches} ({numberOfVectorBatches * VectorCapacity})");
                Console.Out.WriteLine($"Last batch size: {lastBatchSize}");

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

                        var realBatch = new float[VectorCapacity];
                        var imagBatch = new float[VectorCapacity];

                        var batchSize = vectorBatchIndex == numberOfVectorBatches - 1 ? lastBatchSize : VectorCapacity;

                        for (int i = 0; i < batchSize; i++)
                        {
                            realBatch[i] = (float)_c[vectorBatchIndex * VectorCapacity + i].Real;
                            imagBatch[i] = (float)_c[vectorBatchIndex * VectorCapacity + i].Imaginary;
                        }

                        var cReal = new Vector<float>(realBatch);
                        var cImag = new Vector<float>(imagBatch);

                        var vIterations = IteratePoints(cReal, cImag);

                        Console.Out.WriteLine(vIterations);

                        for (int i = 0; i < batchSize; i++)
                        {
                            _iterations[vectorBatchIndex * VectorCapacity + i] = EscapeTime.Choose(vIterations[i]);
                        }
                    });

                return this;
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

        public static Vector<int> IteratePoints(Vector<float> cReal, Vector<float> cImag)
        {
            // TODO: Vectorized MandelbulbChecker

            var zReal = new Vector<float>(0);
            var zImag = new Vector<float>(0);

            var zReal2 = new Vector<float>(0);
            var zImag2 = new Vector<float>(0);

            var oldZReal = new Vector<float>(0);
            var oldZImag = new Vector<float>(0);

            var iterations = Vector<int>.Zero;
            var increment = Vector<int>.One;

            int stepsTaken = 0;
            int stepLimit = 2;

            var inCycle = Vector<int>.Zero;

            while (increment != Vector<int>.Zero)
            {
                stepsTaken++;

                // Doing the two multiplications with an addition is somehow faster than 2 * zReal * zImag!
                // I don't get it either
                zImag = zReal * zImag + zReal * zImag + cImag;
                zReal = zReal2 - zImag2 + cReal;

                zReal2 = zReal * zReal;
                zImag2 = zImag * zImag;

                if (stepsTaken == stepLimit)
                {
                    oldZReal = zReal;
                    oldZImag = zImag;
                    stepsTaken = 0;
                    stepLimit = stepLimit << 1;
                }

                inCycle = inCycle | (Vector.Equals(oldZReal, zReal) & Vector.Equals(oldZImag, zImag));

                var noCycle = Vector.Xor(inCycle, Vector<int>.One);
                var insideCircle = Vector.LessThanOrEqual(zReal2 + zImag2, new Vector<float>(4));

                var shouldContinue = noCycle & insideCircle;

                increment = increment & shouldContinue;
                iterations += increment;
            }

            var mask = inCycle * new Vector<int>(int.MaxValue);
            return iterations + mask;
        }
    }
}

