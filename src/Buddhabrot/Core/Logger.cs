using System;

namespace Buddhabrot.Core
{
    public sealed class Logger : ILog
    {
        private static int _count = 0;
        private readonly ConsoleColor _color;

        public static ILog Create<T>() => new Logger(typeof(T).Name);
        public static ILog Create(string context) => new Logger(context);

        private readonly string _context;

        private Logger(string context)
        {
            _context = context;

            ConsoleColor PickColor()
            {
                switch (_count++ % 5)
                {
                    case 0:
                        return ConsoleColor.Green;
                    case 1:
                        return ConsoleColor.Cyan;
                    case 2:
                        return ConsoleColor.Red;
                    case 3:
                        return ConsoleColor.Magenta;
                    case 4:
                        return ConsoleColor.Yellow;
                    default:
                        throw new Exception("WHAT");
                }
            }

            _color = PickColor();
        }

        public void Info(string message)
        {
            Console.ForegroundColor = _color;
            Console.WriteLine(DateTime.Now.ToString("s") + " " + _context + ": " + message);
            Console.ResetColor();
        }
    }
}
