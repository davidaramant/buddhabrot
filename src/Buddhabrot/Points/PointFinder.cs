using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Buddhabrot.Edges;
using Buddhabrot.IterationKernels;
using log4net;

namespace Buddhabrot.Points
{
    sealed class PointFinder : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(nameof(PointFinder));

        private readonly string _edgesFilePath;
        private readonly IKernel _worker;
        private readonly PointWriter _writer;
        private readonly PointFinderStatistics _statistics = new PointFinderStatistics();

        public PointFinder(string edgesFilePath, string outputFileName)
        {
            _edgesFilePath = edgesFilePath;
            _worker = new VectorKernel();

            _writer = new PointWriter(outputFileName);
        }

        public Task Start(CancellationToken token)
        {
            Log.Info($"Finding points with range: {Constant.IterationRange}");

            var task = new Task(
                () =>
                {
                    using (var edgeReader = EdgeAreas.Load(_edgesFilePath))
                    {
                        var workBatch = new WorkBatch(_worker.GetBatch);

                        var workRemaining = new WorkRemaining(edgeReader.GetPointPairs());

                        int batchNumber = 0;
                        while (!token.IsCancellationRequested)
                        {
                            batchNumber++;

                            workBatch.Reset();

                            // TODO - can this be done in parallel?
                            foreach (var workItem in workRemaining.Take(workBatch.Capacity))
                            {
                                workBatch.Add(workItem);
                            }

                            if (workBatch.Count == 0)
                            {
                                Log.Info("Ran out of work!");
                                break;
                            }

                            workBatch.Compute(token);

                            int numMax = 0;
                            long iterationTotal = 0;

                            IEnumerable<PointPair> ProcessResult(PointPair pair, Complex middle, long iterations)
                            {
                                if (Constant.IterationRange.IsInside(iterations))
                                {
                                    _writer.Save(middle);
                                }
                                else if (iterations == Constant.IterationRange.Max)
                                {
                                    Interlocked.Increment(ref numMax);

                                    var newPair = new PointPair(
                                        inSet: middle,
                                        notInSet: pair.NotInSet);

                                    yield return newPair;
                                }
                                else
                                {
                                    Interlocked.Add(ref iterationTotal, iterations);

                                    var newPair = new PointPair(
                                        inSet: pair.InSet,
                                        notInSet: middle);

                                    yield return newPair;
                                }
                            }

                            workRemaining.AddAdditional(
                                workBatch.GetResults().
                                AsParallel().
                                SelectMany(result => ProcessResult(result.pair, result.middle, result.iterations)));

                            if (batchNumber % 10 == 0)
                            {
                                Log.Debug($"Batch Num: {batchNumber}, Num Max: {numMax:N0}, It avg: {(double)iterationTotal / (workBatch.Count - numMax):N1}");
                            }

                            _statistics.AddPointCount(workBatch.Count);
                        }
                    }

                    Log.Info("Exiting gracefully");
                },
                TaskCreationOptions.LongRunning);
            task.Start();
            return task;
        }

        public void Dispose()
        {
            _worker.Dispose();
            _statistics.Dispose();
        }
    }
}
