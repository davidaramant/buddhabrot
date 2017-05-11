using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Buddhabrot.Edges;
using Buddhabrot.EdgeSpans;
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
        private readonly PointFinderStatistics _statistics;

        public PointFinder(string edgesFilePath, string outputFileName)
        {
            _edgesFilePath = edgesFilePath;
            _worker = new VectorKernel();

            _writer = new PointWriter(outputFileName);
            _statistics = new PointFinderStatistics(_writer);
        }

        public Task Start(CancellationToken token)
        {
            Log.Info($"Finding points with range: {Constant.IterationRange}");

            var task = new Task(
                () =>
                {
                    using (var edgeReader = EdgeAreas.Load(_edgesFilePath))
                    using (var workRemaining = new WorkRemaining(edgeReader.GetEdgeSpans()))
                    {
                        var workBatch = new WorkBatch(_worker.GetBatch);

                        const int batchFrequency = 50;
                        int batchNumber = 0;

                        while (!token.IsCancellationRequested)
                        {
                            batchNumber++;
                            workBatch.Reset();

                            Log.Debug($"Starting batch {batchNumber}");

                            foreach (var workItem in workRemaining.Take(workBatch.Capacity))
                            {
                                workBatch.Add(workItem);
                                if (batchNumber % batchFrequency == 0)
                                {
                                    Log.Debug($"{workItem} => {(workItem.InSet - workItem.NotInSet).Magnitude}");
                                }
                            }

                            if (workBatch.Count == 0)
                            {
                                Log.Info("Ran out of work!");
                                break;
                            }

                            workBatch.Compute(token, Constant.IterationRange.Max);

                            IEnumerable<EdgeSpan> ProcessResult(EdgeSpan span, Complex middle, long iterations)
                            {
                                if (batchNumber % batchFrequency == 0)
                                {
                                    Log.Debug($"Iterations: {iterations:N0}");
                                }

                                if (Constant.IterationRange.IsInside(iterations))
                                {
                                    _writer.Save(middle);
                                }
                                else if (iterations == Constant.IterationRange.Max)
                                {
                                    yield return new EdgeSpan(
                                        inSet: middle,
                                        notInSet: span.NotInSet);
                                }
                                else
                                {
                                    yield return new EdgeSpan(
                                        inSet: span.InSet,
                                        notInSet: middle);
                                }
                            }

                            workRemaining.AddAdditional(
                                workBatch.GetResults().
                                //AsParallel().
                                SelectMany(result => ProcessResult(result.span, result.middle, result.iterations)));

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
