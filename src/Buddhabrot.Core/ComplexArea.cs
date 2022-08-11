using System.Numerics;

namespace Buddhabrot.Core;

public readonly record struct ComplexArea(
    Range RealRange,
    Range ImagRange)
{
    public static ComplexArea SquareFromLowerLeft(Complex lowerLeft, double sideLength) =>
        new(Range.FromMinAndLength(lowerLeft.Real, sideLength),
            Range.FromMinAndLength(lowerLeft.Imaginary, sideLength));
    
    public bool Contains(Complex number) =>
        RealRange.Contains(number.Real) &&
        ImagRange.Contains(number.Imaginary);

    public bool OverlapsWith(ComplexArea otherArea) =>
        RealRange.OverlapsWith(otherArea.RealRange) &&
        ImagRange.OverlapsWith(otherArea.ImagRange);

    public ComplexArea GetNW() => new(RealRange.FirstHalf(), ImagRange.LastHalf());
    public ComplexArea GetNE() => new(RealRange.LastHalf(), ImagRange.LastHalf());
    public ComplexArea GetSE() => new(RealRange.LastHalf(), ImagRange.FirstHalf());
    public ComplexArea GetSW() => new(RealRange.FirstHalf(), ImagRange.FirstHalf());
}