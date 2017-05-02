namespace Buddhabrot.Core
{
    public sealed class ComplexArea
    {
        public Range RealRange { get; }
        public Range ImagRange { get; }

        public ComplexArea(Range realRange, Range imagRange)
        {
            RealRange = realRange;
            ImagRange = imagRange;
        }

        public bool IsInside(FComplex number) =>
                RealRange.IsInside(number.Real) &&
                ImagRange.IsInside(number.Imag);

        public override string ToString() => $"Real: {RealRange}, Imag: {ImagRange}";
    }
}
