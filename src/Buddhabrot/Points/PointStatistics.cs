using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using log4net;

namespace Buddhabrot.Points
{
    sealed class PointStatistics : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(nameof(PointStatistics));
        private readonly Stopwatch _timer = Stopwatch.StartNew();
        private long _pointTotal;

        public void AddPointCount(int count)
        {
            Interlocked.Add(ref _pointTotal, count);
        }

        public void Dispose()
        {
            _timer.Stop();
            var totalRate = _pointTotal / _timer.Elapsed.TotalSeconds;
            Log.Info($"Total time spent: {_timer.Elapsed.Humanize(2)}");
            Log.Info($"Total points processed: {_pointTotal:N0}");
            Log.Info($"Total rate: {totalRate:N1} points/second");
        }
    }
}
