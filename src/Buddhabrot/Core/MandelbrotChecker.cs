using System.Numerics;

// The cycle-detection algorithm requires direct floating point comparisons.
// ReSharper disable CompareOfFloatsByEqualityOperator

namespace Buddhabrot.Core
{
    public static class MandelbrotChecker
    {
        /// <summary>
        /// Determines whether the point is in the set, without using an arbitrary iteration limit.
        /// </summary>
        /// <param name="c">The c.</param>
        /// <remarks>
        /// Brent's Algorithm is used to detect cycles for points in the set.
        /// </remarks>
        /// <returns>
        /// The <see cref="EscapeTime"/> of the point.
        /// </returns>
        public static EscapeTime FindEscapeTimeDouble(Complex c)
        {
            if (MandelbulbChecker.IsInsideBulbs(c))
                return EscapeTime.Infinite;

            var zReal = 0.0;
            var zImag = 0.0;

            var z2Real = 0.0;
            var z2Imag = 0.0;

            var oldZReal = 0.0;
            var oldZImag = 0.0;

            int stepsTaken = 0;
            int stepLimit = 2;

            int iterations = 0;
            while ((z2Real + z2Imag) <= 4)
            {
                iterations++;
                stepsTaken++;

                zImag = 2 * zReal * zImag + c.Imaginary;
                zReal = z2Real - z2Imag + c.Real;

                z2Real = zReal * zReal;
                z2Imag = zImag * zImag;

                if (oldZReal == zReal && oldZImag == zImag)
                    return EscapeTime.Infinite;

                if (stepsTaken == stepLimit)
                {
                    oldZReal = zReal;
                    oldZImag = zImag;
                    stepsTaken = 0;
                    stepLimit = stepLimit << 1;
                }
            }

            return EscapeTime.Discrete(iterations);
        }

        /// <summary>
        /// Determines whether the point is in the set, without using an arbitrary iteration limit.
        /// </summary>
        /// <param name="c">The c.</param>
        /// <remarks>
        /// Brent's Algorithm is used to detect cycles for points in the set.
        /// </remarks>
        /// <returns>
        /// The <see cref="EscapeTime"/> of the point.
        /// </returns>
        public static EscapeTime FindEscapeTimeFloat(Complex c)
        {
            if (MandelbulbChecker.IsInsideBulbs(c))
                return EscapeTime.Infinite;

            var zReal = 0.0f;
            var zImag = 0.0f;

            var z2Real = 0.0f;
            var z2Imag = 0.0f;

            var oldZReal = 0.0f;
            var oldZImag = 0.0f;

            var cReal = (float)c.Real;
            var cImag = (float)c.Imaginary;

            int stepsTaken = 0;
            int stepLimit = 2;

            int iterations = 0;
            while ((z2Real + z2Imag) <= 4)
            {
                iterations++;
                stepsTaken++;

                zImag = 2 * zReal * zImag + cImag;
                zReal = z2Real - z2Imag + cReal;

                z2Real = zReal * zReal;
                z2Imag = zImag * zImag;

                if (oldZReal == zReal && oldZImag == zImag)
                    return EscapeTime.Infinite;

                if (stepsTaken == stepLimit)
                {
                    oldZReal = zReal;
                    oldZImag = zImag;
                    stepsTaken = 0;
                    stepLimit = stepLimit << 1;
                }
            }

            return EscapeTime.Discrete(iterations);
        }
    }
}
