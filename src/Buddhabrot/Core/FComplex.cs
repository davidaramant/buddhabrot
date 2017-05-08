namespace Buddhabrot.Core
{
    /// <summary>
    /// A complex number using floats.
    /// </summary>
    public struct FComplex
    {
        public readonly float Real;
        public readonly float Imag;

        public FComplex(float real, float imag)
        {
            Real = real;
            Imag = imag;
        }

        public static FComplex MidPointOf(FComplex a, FComplex b)
        {
            return new FComplex(
                MidPointOf(a.Real, b.Real),
                MidPointOf(a.Imag, b.Imag));
        }

        private static float MidPointOf(float a, float b)
        {
            return a + 0.5f * (b - a);
        }
    }
}
