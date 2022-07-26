namespace Buddhabrot.Core;

public readonly record struct EscapeTime(int Iterations)
{
    public static readonly EscapeTime Infinite = new(-1);

    public bool IsInfinite => Iterations == -1;

    public static EscapeTime Discrete(int iterations)
    {
        if (iterations < 0)
            throw new ArgumentOutOfRangeException();
        
        return new(iterations);
    }
    public static EscapeTime Choose(int iterations) => iterations < 0 ? Infinite : new EscapeTime(iterations);

    public override string ToString() => IsInfinite ? "Infinite" : Iterations.ToString("N0");
}