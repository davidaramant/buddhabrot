namespace Buddhabrot.Core;

public readonly record struct EscapeTime(int Iterations)
{
    public static readonly EscapeTime Infinite = new(int.MaxValue);

    public bool IsInfinite => this == Infinite;

    public override string ToString() => IsInfinite ? "Infinite" : Iterations.ToString("N0");
}
