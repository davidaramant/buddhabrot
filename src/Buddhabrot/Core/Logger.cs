namespace Buddhabrot.Core;

public sealed class Logger : ILog
{
    private static int _count = 0;
    private readonly ConsoleColor _color;
    private readonly string _context;

    public static ILog Create<T>() => new Logger(typeof(T).Name);
    public static ILog Create(string context) => new Logger(context);

    private Logger(string context)
    {
        _context = context;

        ConsoleColor PickColor() =>
            (_count++ % 5) switch
            {
                0 => ConsoleColor.Green,
                1 => ConsoleColor.Cyan,
                2 => ConsoleColor.Red,
                3 => ConsoleColor.Magenta,
                4 => ConsoleColor.Yellow,
                _ => throw new Exception("WHAT")
            };

        _color = PickColor();
    }

    public void Info(string message)
    {
        Console.ForegroundColor = _color;
        Console.WriteLine(DateTime.Now.ToString("s") + " " + _context + ": " + message);
        Console.ResetColor();
    }
}