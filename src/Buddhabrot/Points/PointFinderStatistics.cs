using System;
using System.Diagnostics;
using System.Threading;
using Buddhabrot.Core;
using Humanizer;

namespace Buddhabrot.Points
{
    sealed class PointFinderStatistics : IDisposable
    {
        private readonly PointWriter _writer;
        private static readonly ILog Log = Logger.Create<PointFinderStatistics>();
        private readonly Stopwatch _timer = Stopwatch.StartNew();
        private long _pointTotal;
        private readonly TimeSpan _reportingInterval = TimeSpan.FromSeconds(30);
        private readonly Timer _reportingTimer;

        public PointFinderStatistics(PointWriter writer)
        {
            _writer = writer;
            _reportingTimer = new Timer(LogRate,null,_reportingInterval,_reportingInterval);
        }

        private void LogRate(object notUsed)
        {
            var totalRate = _pointTotal / _timer.Elapsed.TotalSeconds;
            Log.Info($"Time spent: {_timer.Elapsed.Humanize(2)}");
            Log.Info($"Points processed: {_pointTotal:N0}");
            Log.Info($"Rate: {totalRate:N1} points/second");
            Log.Info($"Points written: {_writer.Count}");
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
