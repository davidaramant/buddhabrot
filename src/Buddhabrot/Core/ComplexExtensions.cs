using System.Numerics;

namespace Buddhabrot.Core
{
    public static class ComplexExtensions
    {
        public static double MagnitudeSquared(this Complex c) => c.Real * c.Real + c.Imaginary * c.Imaginary;

        public static FComplex ToFloat(this Complex c) => new FComplex((float)c.Real, (float)c.Imaginary);
    }
}
