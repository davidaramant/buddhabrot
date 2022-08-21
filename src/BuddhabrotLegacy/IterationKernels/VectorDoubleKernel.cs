using System.Numerics;
using Buddhabrot.Core;

namespace Buddhabrot.IterationKernels;

public static class VectorDoubleKernel
{
    public static int Capacity => Vector<double>.Count;

    public static void FindEscapeTimes(double[] cReals, double[] cImags, int maxIterations, EscapeTime[] escapeTimes)
    {
        var cReal = new Vector<double>(cReals);
        var cImag = new Vector<double>(cImags);

        var vIterations = IteratePoints(cReal, cImag, maxIterations);

        for (int i = 0; i < Capacity; i++)
        {
            escapeTimes[i] = EscapeTime.Choose((int)vIterations[i]);
        }
    }

    public static Vector<long> IteratePoints(Vector<double> cReal, Vector<double> cImag, long maxIterations)
    {
        var zReal = new Vector<double>(0);
        var zImag = new Vector<double>(0);

        // Cache the squares
        // They are used to find the magnitude; reuse these values when computing the next re/im
        var zReal2 = new Vector<double>(0);
        var zImag2 = new Vector<double>(0);

        var iterations = Vector<long>.Zero;
        var increment = Vector<long>.One;

        for (int i = 0; i < maxIterations; i++)
        {
            // Doing the two multiplications with an addition is somehow faster than 2 * zReal * zImag!
            // I don't get it either
            zImag = zReal * zImag + zReal * zImag + cImag;
            zReal = zReal2 - zImag2 + cReal;

            zReal2 = zReal * zReal;
            zImag2 = zImag * zImag;

            var shouldContinue = Vector.LessThanOrEqual(zReal2 + zImag2, new Vector<double>(4));

            increment = increment & shouldContinue;
            iterations += increment;
        }

        return iterations;
    }
}