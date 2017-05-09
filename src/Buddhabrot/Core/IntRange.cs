namespace Buddhabrot.Core
{
    public sealed class IntRange
    {
        public long Min { get; }
        public long Max { get; }
        public long StepSize { get; }
        public bool ExclusiveMax { get; }

        public IntRange(long min, long max, long stepSize = 1, bool maxIsExclusive = true)
        {
            Min = min;
            Max = max;
            StepSize = stepSize;
            ExclusiveMax = maxIsExclusive;
        }

        public bool IsInside(long value) =>
            value >= Min &&
            (ExclusiveMax ? value < Max : value <= Max);

        public override string ToString() => $"[{Min:N0}, {Max:N0}" + (ExclusiveMax ? ")" : "]");
    }
}
