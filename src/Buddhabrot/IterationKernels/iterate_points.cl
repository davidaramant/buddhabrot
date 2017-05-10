#if defined(cl_khr_fp64)
#pragma OPENCL EXTENSION cl_khr_fp64 : enable
#endif


kernel void iterate_points(
    constant const double* cReals,
    constant const double* cImags,
    global int* finalIterations,
    const int maxIterations)
{
    // Initialize z
    double zReal = 0;
    double zImag = 0;

    // Initialize z2
    double zReal2 = 0;
    double zImag2 = 0;

    size_t globalId = get_global_id(0);

    double cReal = cReals[globalId];
    double cImag = cImags[globalId];

    int iterations = 0;
    int increment = 1;

    printf("0 %.15f %.15f\n",cReal,cImag);
    int outCount = 1;

    for (int i = 0; i < maxIterations; i++)
    {
        if( i % 10000 == 0)
        {
            printf("%i %.15f %.15f\n",outCount,zReal,zImag);
            outCount++;
        }

        zImag = 2 * zReal * zImag + cImag;
        zReal = zReal2 - zImag2 + cReal;

        zReal2 = zReal * zReal;
        zImag2 = zImag * zImag;

        // Only increment if the point is still inside the circle
        increment = increment & ((zReal2 + zImag2) <= 4.0);
        iterations += increment;
    }

    finalIterations[globalId] = iterations;
}