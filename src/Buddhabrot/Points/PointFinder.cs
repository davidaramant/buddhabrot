﻿using System;
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
                    {
                        var workBatch = new WorkBatch(_worker.GetBatch);

                        var workRemaining = new WorkRemaining(edgeReader.GetPointPairs());

                        while (!token.IsCancellationRequested)
                        {
                            workBatch.Reset();

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


                            IEnumerable<PointPair> ProcessResult(PointPair pair, Complex middle, long iterations)
                            {
                                if (Constant.IterationRange.IsInside(iterations))
                                {
                                    _writer.Save(middle);
                                }
                                else if (iterations == Constant.IterationRange.Max)
                                {
                                    var newPair = new PointPair(
                                        inSet: middle,
                                        notInSet: pair.NotInSet);

                                    yield return newPair;
                                }
                                else
                                {
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
