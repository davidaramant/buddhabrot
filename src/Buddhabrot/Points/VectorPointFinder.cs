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
    sealed class VectorPointFinder : PointFinder
    {
        private static readonly ILog Log = LogManager.GetLogger(nameof(VectorPointFinder));

        private const int TrialSize = 5;
        private int _trialNumber = 1;
        private readonly double[] _trialSpeeds = new double[TrialSize];

        enum Mutation
        {
            Smaller,
            Larger
        }

        private int _batchSize = 256;
        private double _lastSpeed = 0;
        private Mutation _lastMutation = Mutation.Larger;
        private readonly IntRange _batchRange = new IntRange(8, 2048, stepSize: 8, maxIsExclusive: false);

        private int VectorCapacity => Vector<float>.Count;

        public VectorPointFinder(
            RandomPointGenerator numberGenerator,
            IntRange iterationRange,
            string outputDirectory,
            PointStatistics statistics) :
            base(numberGenerator, iterationRange, outputDirectory, statistics)
        {
        }

        protected override void IteratePointBatch(CancellationToken token)
        {
            var timer = Stopwatch.StartNew();
            var points = NumberGenerator.GetPoints(_batchSize);

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

            timer.Stop();

            Statistics.AddPointCount(_batchSize);

            var pointsPerSecond = _batchSize / timer.Elapsed.TotalSeconds;
            Log.Info($"Processed {pointsPerSecond:N1} pts/s.");

            _trialSpeeds[_trialNumber - 1] = pointsPerSecond;

            if (_trialNumber < TrialSize)
            {
                _trialNumber++;
            }
            else
            {
                var trialAverage = _trialSpeeds.Average();
                var faster = trialAverage > _lastSpeed;

                Log.Info($"Trial average: {trialAverage:N1} pts/s, previously {_lastSpeed:N1} pts/s.");

                var newMutation = _lastMutation;

                if (!faster)
                {
                    newMutation = _lastMutation == Mutation.Larger ? Mutation.Smaller : Mutation.Larger;
                }

                _batchSize =
                    newMutation == Mutation.Larger
                        ? Math.Min(_batchRange.Max, _batchSize + _batchRange.StepSize)
                        : Math.Max(_batchRange.Min, _batchSize - _batchRange.StepSize);

                _lastMutation = newMutation;
                _lastSpeed = trialAverage;
                _trialNumber = 1;

                Log.Info($"Making batch {newMutation} to {_batchSize}");
            }
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
