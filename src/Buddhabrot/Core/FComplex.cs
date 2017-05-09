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

        public override string ToString() => $"{Real} + {Imag}i";
    }
}
