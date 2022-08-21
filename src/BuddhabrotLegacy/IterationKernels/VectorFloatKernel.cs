using System.Numerics;
using Buddhabrot.Core;

namespace Buddhabrot.IterationKernels;

public sealed class VectorFloatKernel
{
    public static int VectorCapacity => Vector<float>.Count;

    public static void FindEscapeTimes(float[] cReals, float[] cImags, int maxIterations, EscapeTime[] escapeTimes)
    {
        var cReal = new Vector<float>(cReals);
        var cImag = new Vector<float>(cImags);

        var vIterations = IteratePoints(cReal, cImag, maxIterations);

        for (int i = 0; i < VectorCapacity; i++)
        {
            escapeTimes[i] = EscapeTime.Choose(vIterations[i]);
        }
    }

    public static Vector<int> IteratePoints(Vector<float> cReal, Vector<float> cImag, int maxIterations)
    {
        var zReal = new Vector<float>(0);
        var zImag = new Vector<float>(0);

        // Cache the squares
        // They are used to find the magnitude; reuse these values when computing the next re/im
        var zReal2 = new Vector<float>(0);
        var zImag2 = new Vector<float>(0);

        var iterations = Vector<int>.Zero;
        var increment = Vector<int>.One;

        for (int i = 0; i < maxIterations; i++)
        {
            // Doing the two multiplications with an addition is somehow faster than 2 * zReal * zImag!
            // I don't get it either
            zImag = zReal * zImag + zReal * zImag + cImag;
            zReal = zReal2 - zImag2 + cReal;

            zReal2 = zReal * zReal;
            zImag2 = zImag * zImag;

            var shouldContinue = Vector.LessThanOrEqual(zReal2 + zImag2, new Vector<float>(4));

            increment = increment & shouldContinue;
            iterations += increment;
        }

        return iterations;
    }
}