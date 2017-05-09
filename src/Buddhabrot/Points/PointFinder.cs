using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Buddhabrot.Core;
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
                        // TODO: Move todo and done into some kind of batch/results wrapper
                        var results = NoResults.Instance;
                        var todo = new List<PointPair>();
                        var done = new List<PointPair>();

                        void SwapTodoAndDone()
                        {
                            var temp = done;
                            done = todo;
                            todo = temp;
                            todo.Clear();
                        }
     
                        var workRemaining = new WorkRemaining(edgeReader.GetPointPairs());

                        while (!token.IsCancellationRequested )
                        {
                            Log.Debug("Starting new batch...");
                            var batch = _worker.GetBatch();
                            Log.Debug($"Batch Count = {batch.Count}, Capacity = {batch.Capacity}");

                            // TODO - can this be done in parallel?
                            foreach (var workItem in workRemaining.Take(batch.Capacity))
                            {
                                todo.Add(workItem);
                                batch.AddPoint(workItem.GetMidPoint());
                            }

                            if (batch.Count == 0)
                            {
                                Log.Info("Ran out of work!");
                                break;
                            }

                            // TODO: Should ComputeIterations be a Task?
                            results = batch.ComputeIterations(token);

                            SwapTodoAndDone();

                            IEnumerable<PointPair> ProcessResult(Tuple<int, int> batchRange)
                            {
                                for (int i = batchRange.Item1; i < batchRange.Item2; i++)
                                {
                                    var iterations = results.GetIteration(i);

                                    if (Constant.IterationRange.IsInside(iterations))
                                    {
                                        _writer.Save(results.GetPoint(i));
                                    }
                                    else if (iterations == Constant.IterationRange.Max)
                                    {
                                        var newPair = new PointPair(
                                            inSet: results.GetPoint(i),
                                            notInSet: done[i].NotInSet);

                                        yield return newPair;
                                    }
                                    else
                                    {
                                        var newPair = new PointPair(
                                            inSet: done[i].InSet,
                                            notInSet: results.GetPoint(i));

                                        yield return newPair;
                                    }
                                }
                            }

                            workRemaining.AddAdditional(
                                Partitioner.Create(0, results.Count).
                                AsParallel().
                                SelectMany(ProcessResult));

                            _statistics.AddPointCount(results.Count);
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
