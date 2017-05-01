namespace Buddhabrot.Core
{
    public sealed class IterationRange
    {
        public int InclusiveMin { get; }
        public int ExclusiveMax { get; }

        public IterationRange(int inclusiveMin, int exclusiveMax)
        {
            InclusiveMin = inclusiveMin;
            ExclusiveMax = exclusiveMax;
        }

        public bool IsInside(int escapeTime) => 
            escapeTime >= InclusiveMin && 
            escapeTime < ExclusiveMax;

        public override string ToString() => $"[{InclusiveMin:N0},{ExclusiveMax:N0})";
    }
}
