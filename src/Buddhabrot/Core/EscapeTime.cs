namespace Buddhabrot.Core;

public readonly struct EscapeTime
{
    public static readonly EscapeTime Infinite = new(-1);

    public bool IsInfinite => _iterations < 0;

    private readonly int _iterations;

    public int Iterations
    {
        get
        {
            if (IsInfinite)
                throw new InvalidOperationException();
            return _iterations;
        }
    }

    private EscapeTime(int iterations) => _iterations = iterations;

    public static EscapeTime Discrete(int iterations) => new(iterations);
    public static EscapeTime Choose(int iterations) => iterations < 0 ? Infinite : new EscapeTime(iterations);

    public override string ToString() => IsInfinite ? "Infinite" : _iterations.ToString("N0");
}