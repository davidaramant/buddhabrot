using System;

namespace Buddhabrot.Core
{
    public sealed class Logger : ILog
    {
        public static ILog Create<T>() => new Logger(typeof(T).Name);
        public static ILog Create(string context) => new Logger(context);

        private readonly string _context;
        private Logger(string context) { _context = context; }

        public void Info(string message) =>
            Console.WriteLine(DateTime.Now.ToString("s") + " " + _context + ": " + message);
    }
}
