namespace Buddhabrot.Core;

public readonly record struct Interval(double InclusiveMin, double ExclusiveMax)
{
    public static readonly Interval Empty = new(0, 0);

    public static Interval FromMinAndLength(double min, double length) => new(min, min + length);

    public static Interval FromCenterAndLength(double center, double length) =>
        new(center - length / 2, center + length / 2);

    public double Magnitude => Math.Abs(ExclusiveMax - InclusiveMin);

    public Interval FirstHalf() => FromMinAndLength(InclusiveMin, Magnitude / 2);

    public Interval LastHalf()
    {
        var halfMagnitude = Magnitude / 2;
        return FromMinAndLength(InclusiveMin + halfMagnitude, halfMagnitude);
    }

    public bool Contains(double value) => value >= InclusiveMin && value < ExclusiveMax;

    public bool OverlapsWith(Interval otherInterval) =>
        Contains(otherInterval.InclusiveMin)
        || Contains(otherInterval.ExclusiveMax)
        || otherInterval.Contains(InclusiveMin)
        || otherInterval.Contains(ExclusiveMax);

    public Interval Intersect(Interval other)
    {
        var min = Math.Max(InclusiveMin, other.InclusiveMin);
        var max = Math.Min(ExclusiveMax, other.ExclusiveMax);

        return min >= max ? Empty : new(min, max);
    }

    public Interval OffsetBy(double delta) => new(InclusiveMin + delta, ExclusiveMax + delta);

    public Interval Scale(double percentage)
    {
        var halfNewMagnitude = (1 - percentage) * Magnitude * 0.5;
        return new Interval(InclusiveMin + halfNewMagnitude, ExclusiveMax - halfNewMagnitude);
    }

    public override string ToString() => $"[{InclusiveMin}, {ExclusiveMax})";
}
