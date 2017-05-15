using System;
using System.Diagnostics;
using System.Threading;
using Humanizer;
using log4net;

namespace Buddhabrot.Utility
{
    sealed class TimedOperation : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(nameof(TimedOperation));
        private readonly Stopwatch _timer = Stopwatch.StartNew();
        private readonly string _workItemName;

        private readonly long _totalWork;
        private long _workDone;

        private readonly TimeSpan _reportingInterval = TimeSpan.FromSeconds(30);
        private readonly Timer _reportingTimer;


        private TimedOperation(string workItemName, long totalWork)
        {
            _workItemName = workItemName;
            _totalWork = totalWork;
            _reportingTimer = new Timer(LogProgress, null, _reportingInterval, _reportingInterval);
        }

        private void LogProgress(object notUsed)
        {
            Log.Info($"--- Time spent: {_timer.Elapsed.Humanize(2)}");

            if (_totalWork != 0)
            {
                var workDone = _workDone;

                if (workDone == 0)
                {
                    Log.Info("No work done yet.");
                }
                else
                {
                    var percentageComplete = (double)workDone / _totalWork;
                    var rate = workDone / _timer.Elapsed.TotalSeconds;

                    var remainingWork = _totalWork - workDone;

                    var estimatedRemainingSeconds = remainingWork / rate;

                    var remaining = TimeSpan.FromSeconds(estimatedRemainingSeconds);

                    Log.Info($"Work done: {workDone:N0}/{_totalWork:N0} ({percentageComplete:P})");
                    Log.Info($"Rate: {rate:N1} {_workItemName}/second");
                    Log.Info($"Estimated end time {DateTime.Now + remaining:t} ({remaining.Humanize(2)} remaining)");
                }
            }
        }

        public void AddWorkDone(int count)
        {
            Interlocked.Add(ref _workDone, count);
        }

        public static TimedOperation Start(
            string workItemName,
            long totalWork = 0) => new TimedOperation(workItemName, totalWork);

        public void Dispose()
        {
            _timer.Stop();
            _reportingTimer.Dispose();
            LogProgress(null);
        }
    }
}
