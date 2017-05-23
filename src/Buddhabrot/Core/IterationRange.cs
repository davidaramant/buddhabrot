namespace Buddhabrot.Core
{
    public sealed class IterationRange
    {
        public int Min { get; }
        public int Max { get; }

        public IterationRange(int min, int max)
        {
            Min = min;
            Max = max;
        }

        public bool IsInside(int value) =>
            value >= Min &&
            value < Max;

        public bool IsInside(EscapeTime escapeTime)
        {
            if (escapeTime.IsInfinite) return false;
            return IsInside(escapeTime.Iterations);
        }

        public override string ToString() => $"[{Min:N0}, {Max:N0})";
    }
}
