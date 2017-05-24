kernel void iterate_points(
    constant const double* cReals,
    constant const double* cImags,
    global int* finalIterations,
    const int maxIterations)
{
    int globalId = (int)get_global_id(0);

    double cReal = cReals[globalId];
    double cImag = cImags[globalId];

    double zReal = 0;
    double zImag = 0;

    double zReal2 = 0;
    double zImag2 = 0;

    int iterations = 0;

    for (int i = 0; i < maxIterations; i++)
    {
        zImag = 2 * zReal * zImag + cImag;
        zReal = zReal2 - zImag2 + cReal;

        zReal2 = zReal * zReal;
        zImag2 = zImag * zImag;

        // Only increment if the point is still inside the circle
        iterations += (zReal2 + zImag2) <= 4.0;
    }

    finalIterations[globalId] = iterations;
}