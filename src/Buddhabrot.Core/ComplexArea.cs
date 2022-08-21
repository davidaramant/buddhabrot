using System.Numerics;

namespace Buddhabrot.Core;

public readonly record struct ComplexArea(
    Interval RealInterval,
    Interval ImagInterval)
{
    public double Width => RealInterval.Magnitude;
    public double Height => ImagInterval.Magnitude;
    
    public static readonly ComplexArea Empty = new(Interval.Empty, Interval.Empty);

    public Complex TopLeftCorner => new(RealInterval.InclusiveMin, ImagInterval.ExclusiveMax);

    public bool Contains(Complex number) =>
        RealInterval.Contains(number.Real) &&
        ImagInterval.Contains(number.Imaginary);

    public bool OverlapsWith(ComplexArea otherArea) =>
        RealInterval.OverlapsWith(otherArea.RealInterval) &&
        ImagInterval.OverlapsWith(otherArea.ImagInterval);

    public ComplexArea Intersect(ComplexArea otherArea)
    {
        var realIntersection = RealInterval.Intersect(otherArea.RealInterval);
        var imagIntersection = ImagInterval.Intersect(otherArea.ImagInterval);

        if (realIntersection == Interval.Empty || imagIntersection == Interval.Empty)
            return Empty;

        return new(realIntersection, imagIntersection);
    }

    public ComplexArea OffsetBy(double realDelta, double imagDelta) =>
        new(RealInterval.OffsetBy(realDelta), ImagInterval.OffsetBy(imagDelta));

    public ComplexArea Scale(double scale) =>
        new(RealInterval.Scale(scale), ImagInterval.Scale(scale));

    public ComplexArea GetSWQuadrant() => new(RealInterval.FirstHalf(), ImagInterval.FirstHalf());
    public ComplexArea GetNWQuadrant() => new(RealInterval.FirstHalf(), ImagInterval.LastHalf());
    public ComplexArea GetNEQuadrant() => new(RealInterval.LastHalf(), ImagInterval.LastHalf());
    public ComplexArea GetSEQuadrant() => new(RealInterval.LastHalf(), ImagInterval.FirstHalf());

    public override string ToString() => $"R:{RealInterval}, I:{ImagInterval}";
}