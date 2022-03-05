using System;

namespace Buddhabrot.Core;

public sealed class DoubleRange
{
    public double InclusiveMin { get; }
    public double ExclusiveMax { get; }

    public double Magnitude => Math.Abs(ExclusiveMax - InclusiveMin);

    public DoubleRange(double inclusiveMin, double exclusiveMax)
    {
        InclusiveMin = inclusiveMin;
        ExclusiveMax = exclusiveMax;
    }

    public bool IsInside(double value) =>
        value >= InclusiveMin &&
        value < ExclusiveMax;

    public override string ToString() => $"[{InclusiveMin}, {ExclusiveMax})";
}