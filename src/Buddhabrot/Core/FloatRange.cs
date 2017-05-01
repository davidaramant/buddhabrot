using System;

namespace Buddhabrot.Core
{
    public sealed class FloatRange
    {
        public float InclusiveMin { get; }
        public float ExclusiveMax { get; }

        public float Magnitude => Math.Abs(ExclusiveMax - InclusiveMin);

        public FloatRange(float inclusiveMin, float exclusiveMax)
        {
            InclusiveMin = inclusiveMin;
            ExclusiveMax = exclusiveMax;
        }

        public bool IsInside(double value)
        {
            return
                value >= InclusiveMin &&
                value < ExclusiveMax;
        }

        public override string ToString() => $"[{InclusiveMin},{ExclusiveMax})";
    }
}
