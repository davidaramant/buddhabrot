namespace Buddhabrot.Core;

public readonly record struct Range(
    double InclusiveMin,
    double ExclusiveMax)
{
    public static Range FromMinAndLength(double min, double length) => new(min, min + length);

    public double Magnitude => Math.Abs(ExclusiveMax - InclusiveMin);

    public Range FirstHalf() => FromMinAndLength(InclusiveMin,Magnitude/2);

    public Range LastHalf()
    {
        var halfMagnitude = Magnitude / 2;
        return FromMinAndLength(InclusiveMin + halfMagnitude, halfMagnitude);
    }
    
    public bool Contains(double value) =>
        value >= InclusiveMin &&
        value < ExclusiveMax;

    public bool OverlapsWith(Range otherRange) => 
        Contains(otherRange.InclusiveMin) ||
        Contains(otherRange.ExclusiveMax) ||
        otherRange.Contains(InclusiveMin) || 
        otherRange.Contains(ExclusiveMax);

    public override string ToString() => $"[{InclusiveMin}, {ExclusiveMax})";
}