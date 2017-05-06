﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Buddhabrot.Core;
using log4net;

namespace Buddhabrot.Points
{
    abstract class PointFinder : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(nameof(PointFinder));
        protected RandomPointGenerator NumberGenerator { get; }
        protected IntRange IterationRange { get; }
        protected PointWriter PointWriter { get; }
        protected PointStatistics Statistics { get; }

        protected PointFinder(
            RandomPointGenerator numberGenerator,
            IntRange iterationRange,
            string outputDirectory, 
            PointStatistics statistics)
        {
            NumberGenerator = numberGenerator;
            IterationRange = iterationRange;
            Statistics = statistics;
            PointWriter = new PointWriter(System.IO.Path.Combine(outputDirectory,$"points{DateTime.Now:yyyyMMdd-HHmmss}"));
        }

        public Task Start(CancellationToken token)
        {
            Log.Info($"Finding points with range: {IterationRange}");

            var task = new Task(
                () =>
                {
                    while (!token.IsCancellationRequested)
                    {
                        IteratePointBatch(token);
                    }

                    Log.Info("Exiting gracefully");
                },
                TaskCreationOptions.LongRunning);
            task.Start();
            return task;
        }


        protected abstract void IteratePointBatch(CancellationToken token);

        protected virtual void Dispose(bool disposing)
        {
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}