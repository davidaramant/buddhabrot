namespace Buddhabrot.Core
{
    public sealed class FloatArea
    {
        public FloatRange RealRange { get; }
        public FloatRange ImagRange { get; }

        public FloatArea(FloatRange realRange, FloatRange imagRange)
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
