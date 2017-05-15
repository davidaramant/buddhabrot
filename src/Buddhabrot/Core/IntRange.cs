namespace Buddhabrot.Core
{
    public sealed class IntRange
    {
        public int Min { get; }
        public int Max { get; }
        public int StepSize { get; }
        public bool ExclusiveMax { get; }

        public IntRange(int min, int max, int stepSize = 1, bool maxIsExclusive = true)
        {
            Min = min;
            Max = max;
            StepSize = stepSize;
            ExclusiveMax = maxIsExclusive;
        }

        public bool IsInside(int value) =>
            value >= Min &&
            (ExclusiveMax ? value < Max : value <= Max);

        public override string ToString() => $"[{Min:N0}, {Max:N0}" + (ExclusiveMax ? ")" : "]");
    }
}
