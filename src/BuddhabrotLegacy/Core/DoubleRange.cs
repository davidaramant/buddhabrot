namespace Buddhabrot.Core;

public sealed record DoubleRange(
    double InclusiveMin,
    double ExclusiveMax)
{
    public double Magnitude => Math.Abs(ExclusiveMax - InclusiveMin);

    public bool IsInside(double value) =>
        value >= InclusiveMin &&
        value < ExclusiveMax;
}