using System.Numerics;

namespace Buddhabrot.Core;

public sealed record ComplexArea(
    Range RealRange,
    Range ImagRange)
{
    public static ComplexArea SquareFromLowerLeft(Complex lowerLeft, double sideLength) =>
        new(Range.FromMinAndLength(lowerLeft.Real, sideLength),
            Range.FromMinAndLength(lowerLeft.Imaginary, sideLength));
    
    public bool IsInside(Complex number) =>
        RealRange.IsInside(number.Real) &&
        ImagRange.IsInside(number.Imaginary);

    public ComplexArea GetPositiveImagArea() => new(RealRange, new Range(0, ImagRange.ExclusiveMax));
}