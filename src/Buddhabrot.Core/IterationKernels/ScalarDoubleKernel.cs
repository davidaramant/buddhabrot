using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace Buddhabrot.Core.IterationKernels;

[SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator", Justification = "Cycle detection requires direct float comparisons.")]
public static class ScalarDoubleKernel 
{
    public static EscapeTime FindEscapeTime(Complex c, int maxIterations)
    {
        if (BulbChecker.IsInsideBulbs(c))
            return EscapeTime.Infinite;

        var zReal = 0.0;
        var zImag = 0.0;

        var z2Real = 0.0;
        var z2Imag = 0.0;

        var oldZReal = 0.0;
        var oldZImag = 0.0;

        int stepsTaken = 0;
        int stepLimit = 2;

        for (int i = 0; i < maxIterations; i++)
        {
            stepsTaken++;

            zImag = 2 * zReal * zImag + c.Imaginary;
            zReal = z2Real - z2Imag + c.Real;

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

    public static EscapeTime FindEscapeTimeNoCycleDetection(Complex c, int maxIterations)
    {
        if (BulbChecker.IsInsideBulbs(c))
            return EscapeTime.Infinite;

        var zReal = 0.0;
        var zImag = 0.0;

        var z2Real = 0.0;
        var z2Imag = 0.0;

        for (int i = 0; i < maxIterations; i++)
        {
            zImag = 2 * zReal * zImag + c.Imaginary;
            zReal = z2Real - z2Imag + c.Real;

            z2Real = zReal * zReal;
            z2Imag = zImag * zImag;

            if ((z2Real + z2Imag) > 4)
            {
                return EscapeTime.Discrete(i);
            }
        }

        return EscapeTime.Infinite;
    }

    public static double FindDistance(Complex c, int maxIterations)
    {
        if (BulbChecker.IsInsideBulbs(c))
            return 0;

        var z = Complex.Zero;

        var oldZ = Complex.Zero;

        var dZ = Complex.Zero;

        int stepsTaken = 0;
        int stepLimit = 2;

        for (int i = 0; i < maxIterations; i++)
        {
            stepsTaken++;

            dZ = 2 * z * dZ + 1;
            z = z * z + c;

            if (oldZ == z)
                return 0;

            if (z.MagnitudeSquared() > 4)
            {
                break;
            }

            if (stepsTaken == stepLimit)
            {
                oldZ = z;
                stepsTaken = 0;
                stepLimit = stepLimit << 1;
            }
        }

        var magZ = z.Magnitude;
        var magDZ = dZ.Magnitude;
        return 2 * Math.Log(magZ * magZ) * magZ / magDZ;
    }
}