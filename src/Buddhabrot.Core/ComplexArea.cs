using System.Numerics;

namespace Buddhabrot.Core;

public sealed record ComplexArea(
    DoubleRange RealRange,
    DoubleRange ImagRange)
{
    public bool IsInside(Complex number) =>
        RealRange.IsInside(number.Real) &&
        ImagRange.IsInside(number.Imaginary);

    public ComplexArea GetPositiveImagArea() => new(RealRange, new DoubleRange(0, ImagRange.ExclusiveMax));
}