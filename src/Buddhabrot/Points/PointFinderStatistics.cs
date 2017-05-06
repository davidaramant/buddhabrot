using System;
using System.Diagnostics;
using System.Threading;
using Humanizer;
using log4net;

namespace Buddhabrot.Points
{
    sealed class PointFinderStatistics : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(nameof(PointFinderStatistics));
        private readonly Stopwatch _timer = Stopwatch.StartNew();
        private long _pointTotal;
        private readonly TimeSpan _reportingInterval = TimeSpan.FromSeconds(30);
        private readonly Timer _reportingTimer;

        public PointFinderStatistics()
        {
            _reportingTimer = new Timer(LogRate,null,_reportingInterval,_reportingInterval);
        }

        private void LogRate(object notUsed)
        {
            var totalRate = _pointTotal / _timer.Elapsed.TotalSeconds;
            Log.Info($"Time spent: {_timer.Elapsed.Humanize(2)}");
            Log.Info($"Points processed: {_pointTotal:N0}");
            Log.Info($"Rate: {totalRate:N1} points/second");
        }

        public void AddPointCount(int count)
        {
            Interlocked.Add(ref _pointTotal, count);
        }

        public void Dispose()
        {
            _timer.Stop();
            _reportingTimer.Dispose();
            LogRate(null);
        }
    }
}
