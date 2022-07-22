namespace Buddhabrot.Core;

public sealed record IterationRange(
    int Min, 
    int Max)
{
    public bool IsInside(int value) =>
        value >= Min &&
        value < Max;

    public bool IsInside(EscapeTime escapeTime) => 
        !escapeTime.IsInfinite && IsInside(escapeTime.Iterations);
}