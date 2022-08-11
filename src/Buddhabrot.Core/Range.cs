namespace Buddhabrot.Core;

public sealed record Range(
    double InclusiveMin,
    double ExclusiveMax)
{
    public static Range FromMinAndLength(double min, double length) => new(min, min + length);

    public double Magnitude => Math.Abs(ExclusiveMax - InclusiveMin);

    public bool IsInside(double value) =>
        value >= InclusiveMin &&
        value < ExclusiveMax;

    public bool Intersects(Range otherRange) => 
        IsInside(otherRange.InclusiveMin) ||
        IsInside(otherRange.ExclusiveMax) ||
        otherRange.IsInside(InclusiveMin) || 
        otherRange.IsInside(ExclusiveMax);

    public override string ToString() => $"[{InclusiveMin}, {ExclusiveMax})";
}