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
        private readonly string _description;

        // TODO: Figure out how this should be set
        private readonly long _totalWork;
        private long _workDone;

        private readonly TimeSpan _reportingInterval = TimeSpan.FromSeconds(30);
        private readonly Timer _reportingTimer;


        private TimedOperation(string description, bool reportProgress)
        {
            _description = description;

            if (reportProgress)
            {
                _reportingTimer = new Timer(LogProgress, null, _reportingInterval, _reportingInterval);
            }
        }

        private void LogProgress(object notUsed)
        {
            Log.Info($"Time spent: {_timer.Elapsed.Humanize(2)}");


            //var totalRate = _workDone / _timer.Elapsed.TotalSeconds;
            //Log.Info($"Time spent: {_timer.Elapsed.Humanize(2)}");
            //Log.Info($"Points processed: {_workDone:N0}");
            //Log.Info($"Rate: {totalRate:N1} points/second");
            //Log.Info($"Points written: {_writer.Count}");
        }

        public void AddWorkDone(int count)
        {
            Interlocked.Add(ref _workDone, count);
        }

        public static TimedOperation Start(string description, bool reportProgress = true) => new TimedOperation(description, reportProgress);

        public void Dispose()
        {
            _timer.Stop();
            _reportingTimer?.Dispose();
            LogProgress(null);
        }
    }
}
