namespace Buddhabrot.Core
{
    public sealed class FComplexArea
    {
        public FloatRange RealRange { get; }
        public FloatRange ImagRange { get; }

        public FComplexArea(FloatRange realRange, FloatRange imagRange)
        {
            RealRange = realRange;
            ImagRange = imagRange;
        }

        public bool IsInside(FComplex number) =>
            RealRange.IsInside(number.Real) &&
            ImagRange.IsInside(number.Imaginary);

        public override string ToString() => $"Real: {RealRange}, Imag: {ImagRange}";

        public FComplexArea GetPositiveImagArea() => new FComplexArea(RealRange, new FloatRange(0, ImagRange.ExclusiveMax));
    }
}
