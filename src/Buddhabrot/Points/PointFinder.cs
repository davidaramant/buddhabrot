using System;
using System.Threading;
using System.Threading.Tasks;
using Buddhabrot.Core;
using log4net;

namespace Buddhabrot.Points
{
    abstract class PointFinder
    {
        private static readonly ILog Log = LogManager.GetLogger(nameof(PointFinder));
        protected RandomPointGenerator NumberGenerator { get; }
        protected IntRange IterationRange { get; }
        protected PointWriter PointWriter { get; }

        protected PointFinder(
            RandomPointGenerator numberGenerator,
            IntRange iterationRange,
            string outputDirectory)
        {
            NumberGenerator = numberGenerator;
            IterationRange = iterationRange;
            PointWriter = new PointWriter(outputDirectory);
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
    }
}
