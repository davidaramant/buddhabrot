using System.Numerics;

namespace Buddhabrot.Core;

public readonly record struct ComplexArea(
    Range RealRange,
    Range ImagRange)
{
    public double Width => RealRange.Magnitude;
    public double Height => ImagRange.Magnitude;
    
    public static readonly ComplexArea Empty = new(Range.Empty, Range.Empty);

    public Complex TopLeftCorner => new(RealRange.InclusiveMin, ImagRange.ExclusiveMax);

    public bool Contains(Complex number) =>
        RealRange.Contains(number.Real) &&
        ImagRange.Contains(number.Imaginary);

    public bool OverlapsWith(ComplexArea otherArea) =>
        RealRange.OverlapsWith(otherArea.RealRange) &&
        ImagRange.OverlapsWith(otherArea.ImagRange);

    public ComplexArea Intersect(ComplexArea otherArea)
    {
        var realIntersection = RealRange.Intersect(otherArea.RealRange);
        var imagIntersection = ImagRange.Intersect(otherArea.ImagRange);

        if (realIntersection == Range.Empty || imagIntersection == Range.Empty)
            return Empty;

        return new(realIntersection, imagIntersection);
    }

    public ComplexArea OffsetBy(double realDelta, double imagDelta) =>
        new(RealRange.OffsetBy(realDelta), ImagRange.OffsetBy(imagDelta));

    public ComplexArea Scale(double scale) =>
        new(RealRange.Scale(scale), ImagRange.Scale(scale));

    public ComplexArea GetSWQuadrant() => new(RealRange.FirstHalf(), ImagRange.FirstHalf());
    public ComplexArea GetNWQuadrant() => new(RealRange.FirstHalf(), ImagRange.LastHalf());
    public ComplexArea GetNEQuadrant() => new(RealRange.LastHalf(), ImagRange.LastHalf());
    public ComplexArea GetSEQuadrant() => new(RealRange.LastHalf(), ImagRange.FirstHalf());

    public override string ToString() => $"R:{RealRange}, I:{ImagRange}";
}