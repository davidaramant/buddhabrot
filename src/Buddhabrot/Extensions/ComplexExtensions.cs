using System.Numerics;

namespace Buddhabrot.Extensions
{
    public static class ComplexExtensions
    {
        public static double MagnitudeSquared(this Complex c) => c.Real * c.Real + c.Imaginary * c.Imaginary;
    }
}
