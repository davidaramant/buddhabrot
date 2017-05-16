bool isInsideBulbs(double cReal, double cImag);
bool isInLargerBulb(double cReal, double cImag);
bool isInsideCircle(double centerReal, double centerImag, double radius, double cReal, double cImag);

kernel void iterate_points_double(
    constant const double* cReals,
    constant const double* cImags,
    global int* finalIterations,
    const int maxIterations)
{
    size_t globalId = get_global_id(0);

    double cReal = cReals[globalId];
    double cImag = cImags[globalId];

    if (isInsideBulbs(cReal, cImag))
    {
        finalIterations[globalId] = maxIterations;
        return;
    }

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

bool isInsideBulbs(double cReal, double cImag)
{
    return 
        isInLargerBulb(cReal, cImag) ||
        isInsideCircle(-1, 0, 0.25, cReal, cImag) ||
        isInsideCircle(-0.125, 0.744, 0.092, cReal, cImag) ||
        isInsideCircle(-1.308, 0, 0.058, cReal, cImag);
}

bool isInLargerBulb(double cReal, double cImag)
{
    double realMinusFourth = cReal - 0.25;
    double imagSquared = cImag * cImag;
    double q = realMinusFourth * realMinusFourth + imagSquared;

    return (q * (q + realMinusFourth)) < (0.25 * imagSquared);
}

bool isInsideCircle(double centerReal, double centerImag, double radius, double cReal, double cImag)
{
    double translatedReal = cReal - centerReal;
    double translatedImag = cImag - centerImag;

    return (translatedReal*translatedReal + translatedImag*translatedImag) <= (radius * radius);
}

kernel void iterate_points_float(
    constant const float* cReals,
    constant const float* cImags,
    global int* finalIterations,
    const int maxIterations)
{
    size_t globalId = get_global_id(0);

    float cReal = cReals[globalId];
    float cImag = cImags[globalId];

    float zReal = 0;
    float zImag = 0;

    float zReal2 = 0;
    float zImag2 = 0;

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