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
        protected IterationRange IterationRange { get; }
        protected PointWriter PointWriter { get; }

        protected PointFinder(
            RandomPointGenerator numberGenerator,
            IterationRange iterationRange,
            string outputDirectory)
        {
            NumberGenerator = numberGenerator;
            IterationRange = iterationRange;
            PointWriter = new PointWriter(outputDirectory);
        }

        public Task Start(CancellationToken token)
        {
            Log.Info($"Finding points with range: {IterationRange}");

            var task = new Task(()=>IteratePoints(token),TaskCreationOptions.LongRunning);
            task.Start();
            return task;
        }

        protected abstract void IteratePoints(CancellationToken token);
    }
}
