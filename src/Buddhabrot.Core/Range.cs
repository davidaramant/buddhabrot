namespace Buddhabrot.Core;

public readonly record struct Range(
    double InclusiveMin,
    double ExclusiveMax)
{
    public static readonly Range Empty = new(0, 0);
    
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

    public Range Intersect(Range other)
    {
        var min = Math.Max(InclusiveMin, other.InclusiveMin);
        var max = Math.Min(ExclusiveMax, other.ExclusiveMax);

        return min >= max ? Empty : new(min, max);
    }

    public Range OffsetBy(double delta) => new(InclusiveMin + delta, ExclusiveMax + delta);

    public Range Scale(double percentage)
    {
        var halfNewMagnitude = (1-percentage) * Magnitude * 0.5;
        return new Range(InclusiveMin + halfNewMagnitude, ExclusiveMax - halfNewMagnitude);
    }
    
    public override string ToString() => $"[{InclusiveMin}, {ExclusiveMax})";
}