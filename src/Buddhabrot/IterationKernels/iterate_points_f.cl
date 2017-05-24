kernel void iterate_points(
    constant const float* cReals,
    constant const float* cImags,
    global int* finalIterations)
{
    int globalId = (int)get_global_id(0);

    float cReal = cReals[globalId];
    float cImag = cImags[globalId];

    float zReal = 0;
    float zImag = 0;

    float zReal2 = 0;
    float zImag2 = 0;

    int iterations = 0;

    for (int i = 0; i < 15 * 1000 * 1000; i++)
    {
        zImag = 2 * zReal * zImag + cImag;
        zReal = zReal2 - zImag2 + cReal;

        zReal2 = zReal * zReal;
        zImag2 = zImag * zImag;

        iterations += (zReal2 + zImag2) <= 4.0f;
    }

    finalIterations[globalId] = iterations;
}
