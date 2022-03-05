using System.Numerics;

namespace Buddhabrot.Core;

public sealed class ComplexArea
{
    public DoubleRange RealRange { get; }
    public DoubleRange ImagRange { get; }

    public ComplexArea(DoubleRange realRange, DoubleRange imagRange)
    {
        RealRange = realRange;
        ImagRange = imagRange;
    }

    public bool IsInside(Complex number) =>
        RealRange.IsInside(number.Real) &&
        ImagRange.IsInside(number.Imaginary);

    public override string ToString() => $"Real: {RealRange}, Imag: {ImagRange}";

    public ComplexArea GetPositiveImagArea() => new ComplexArea(RealRange, new DoubleRange(0, ImagRange.ExclusiveMax));
}