using System;
using System.Globalization;
using System.Numerics;

namespace Buddhabrot.Core
{
    /// <summary>
    /// A complex number using floats.
    /// </summary>
    /// <remarks>
    /// Adapted from Microsoft reference source for Complex: 
    /// http://referencesource.microsoft.com/#System.Numerics/System/Numerics/Complex.cs
    /// </remarks>
    public struct FComplex : IEquatable<FComplex>
    {
        public readonly float Real;
        public readonly float Imaginary;

        public float Magnitude => FComplex.Abs(this);
        public float MagnitudeSquared() => Real * Real + Imaginary * Imaginary;

        public static readonly FComplex Zero = new FComplex(0.0f, 0.0f);
        public static readonly FComplex One = new FComplex(1.0f, 0.0f);
        public static readonly FComplex ImaginaryOne = new FComplex(0.0f, 1.0f);

        public FComplex(float real, float imaginary)
        {
            Real = real;
            Imaginary = imaginary;
        }

        public Complex ToDouble() => new Complex(Real, Imaginary);

        // --------------SECTION: Arithmetic Operator(unary) Overloading -------------- //
        public static FComplex operator -(FComplex value)  /* Unary negation of a complex number */
        {
            return (new FComplex((-value.Real), (-value.Imaginary)));
        }

        // --------------SECTION: Arithmetic Operator(binary) Overloading -------------- //       
        public static FComplex operator +(FComplex left, FComplex right)
        {
            return (new FComplex((left.Real + right.Real), (left.Imaginary + right.Imaginary)));

        }

        public static FComplex operator -(FComplex left, FComplex right)
        {
            return (new FComplex((left.Real - right.Real), (left.Imaginary - right.Imaginary)));
        }

        public static FComplex operator *(FComplex left, FComplex right)
        {
            // Multiplication:  (a + bi)(c + di) = (ac -bd) + (bc + ad)i
            float result_Realpart = (left.Real * right.Real) - (left.Imaginary * right.Imaginary);
            float result_Imaginarypart = (left.Imaginary * right.Real) + (left.Real * right.Imaginary);
            return (new FComplex(result_Realpart, result_Imaginarypart));
        }

        public static FComplex operator /(FComplex left, FComplex right)
        {
            // Division : Smith's formula.
            float a = left.Real;
            float b = left.Imaginary;
            float c = right.Real;
            float d = right.Imaginary;

            if (Math.Abs(d) < Math.Abs(c))
            {
                float doc = d / c;
                return new FComplex((a + b * doc) / (c + d * doc), (b - a * doc) / (c + d * doc));
            }
            else
            {
                float cod = c / d;
                return new FComplex((b + a * cod) / (d + c * cod), (-a + b * cod) / (d + c * cod));
            }
        }

        public static float Abs(FComplex value)
        {
            if (float.IsInfinity(value.Real) || float.IsInfinity(value.Imaginary))
            {
                return float.PositiveInfinity;
            }

            // |value| == sqrt(a^2 + b^2)
            // sqrt(a^2 + b^2) == a/a * sqrt(a^2 + b^2) = a * sqrt(a^2/a^2 + b^2/a^2)
            // Using the above we can factor out the square of the larger component to dodge overflow.


            float c = Math.Abs(value.Real);
            float d = Math.Abs(value.Imaginary);

            if (c > d)
            {
                float r = d / c;
                return c * (float)Math.Sqrt(1.0f + r * r);
            }
            else if (d == 0.0f)
            {
                return c;  // c is either 0.0 or NaN
            }
            else
            {
                float r = c / d;
                return d * (float)Math.Sqrt(1.0f + r * r);
            }
        }

        public static bool operator ==(FComplex left, FComplex right)
        {
            return ((left.Real == right.Real) && (left.Imaginary == right.Imaginary));
        }
        public static bool operator !=(FComplex left, FComplex right)
        {
            return ((left.Real != right.Real) || (left.Imaginary != right.Imaginary));
        }

        public override bool Equals(object obj)
        {
            if (!(obj is FComplex)) return false;
            return this == ((FComplex)obj);
        }
        public bool Equals(FComplex value)
        {
            return ((this.Real.Equals(value.Real)) && (this.Imaginary.Equals(value.Imaginary)));
        }

        public override String ToString()
        {
            return (String.Format(CultureInfo.CurrentCulture, "({0}, {1})", this.Real, this.Imaginary));
        }

        public override Int32 GetHashCode()
        {
            Int32 n1 = 99999997;
            Int32 hash_real = this.Real.GetHashCode() % n1;
            Int32 hash_imaginary = this.Imaginary.GetHashCode();
            Int32 final_hashcode = hash_real ^ hash_imaginary;
            return (final_hashcode);
        }
    }
}
