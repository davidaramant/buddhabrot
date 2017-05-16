using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Buddhabrot.Core;

namespace Buddhabrot.IterationKernels
{
    [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator", Justification = "Cycle detection requires direct float comparisons.")]
    public sealed class ScalarFloatKernel : IScalarKernel
    {
        EscapeTime IScalarKernel.FindEscapeTime(Complex c, int maxIterations) => FindEscapeTime(c, maxIterations);
        EscapeTime IScalarKernel.FindEscapeTime(Complex c) => FindEscapeTime(c);

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
        public static EscapeTime FindEscapeTime(Complex c)
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

        public static EscapeTime FindEscapeTime(Complex c, int maxIterations)
        {
            if (MandelbulbChecker.IsInsideBulbs(c))
                return EscapeTime.Infinite;

            var zReal = 0.0f;
            var zImag = 0.0f;

            var z2Real = 0.0f;
            var z2Imag = 0.0f;

            var oldZReal = 0.0f;
            var oldZImag = 0.0f;

            int stepsTaken = 0;
            int stepLimit = 2;

            for (int i = 0; i < maxIterations; i++)
            {
                stepsTaken++;

                zImag = 2 * zReal * zImag + (float)c.Imaginary;
                zReal = z2Real - z2Imag + (float)c.Real;

                if (oldZReal == zReal && oldZImag == zImag)
                    return EscapeTime.Infinite;

                z2Real = zReal * zReal;
                z2Imag = zImag * zImag;

                if ((z2Real + z2Imag) > 4)
                {
                    return EscapeTime.Discrete(i);
                }

                if (stepsTaken == stepLimit)
                {
                    oldZReal = zReal;
                    oldZImag = zImag;
                    stepsTaken = 0;
                    stepLimit = stepLimit << 1;
                }
            }

            return EscapeTime.Infinite;
        }
    }
}
