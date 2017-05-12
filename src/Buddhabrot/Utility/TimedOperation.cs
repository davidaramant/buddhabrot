using System;
using System.Diagnostics;
using Humanizer;
using log4net;

namespace Buddhabrot.Utility
{
    sealed class TimedOperation : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(nameof(TimedOperation));
        private readonly Stopwatch _timer = Stopwatch.StartNew();
        private readonly string _description;

        private TimedOperation(string description)
        {
            _description = description;
        }

        public static TimedOperation Start(string description) => new TimedOperation(description);

        public void Dispose()
        {
            _timer.Stop();
            Log.Info($"{_description} took {_timer.Elapsed.Humanize(2)}");
        }
    }
}
