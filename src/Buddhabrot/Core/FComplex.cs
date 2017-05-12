using System;

namespace Buddhabrot.Core
{
    /// <summary>
    /// A float version of <see cref="System.Numerics.Complex"/>
    /// </summary>
    public struct FComplex : IEquatable<FComplex>
    {
        public readonly float Real;
        public readonly float Imaginary;

        public FComplex(float real, float imaginary)
        {
            Real = real;
            Imaginary = imaginary;
        }

        // Match the format for Complex for consistency.
        public override string ToString() => $"({Real}, {Imaginary})";

        public float MagnitudeSquared() => Real * Real + Imaginary * Imaginary;

        #region Equality members

        public bool Equals(FComplex other)
        {
            return Real.Equals(other.Real) && Imaginary.Equals(other.Imaginary);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is FComplex && Equals((FComplex)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Real.GetHashCode() * 397) ^ Imaginary.GetHashCode();
            }
        }

        public static bool operator ==(FComplex left, FComplex right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(FComplex left, FComplex right)
        {
            return !left.Equals(right);
        }

        #endregion Equality members
    }
}
