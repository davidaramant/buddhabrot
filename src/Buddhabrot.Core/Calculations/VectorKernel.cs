using System.Numerics;

namespace Buddhabrot.Core.Calculations;

public static class VectorKernel
{
    public static void FindEscapeTimes(Complex[] points, EscapeTime[] escapeTimes, int numPoints, int maxIterations)
    {
        Parallel.For(
            0,
            numPoints,
            i =>
            {
                escapeTimes[i] = ScalarKernel.FindEscapeTime(points[i], maxIterations);
            }
        );
    }
}
