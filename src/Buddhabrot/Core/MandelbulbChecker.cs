﻿using System.Numerics;

namespace Buddhabrot.Core;

public static class MandelbulbChecker
{
    /// <summary>
    /// Does a fast check to see if a complex number lies within one of the larger bulbs of the Mandelbrot set.
    /// </summary>
    public static bool IsInsideBulbs(double real, double imag) => IsInsideBulbs(new Complex(real, imag));

    /// <summary>
    /// Does a fast check to see if a complex number lies within one of the larger bulbs of the Mandelbrot set.
    /// </summary>
    public static bool IsInsideBulbs(Complex number)
    {
        return
            IsInLargerBulb(number) ||
            IsInCircularBulbs(number);
    }

    public static bool IsInsideBulbs(FComplex number) => IsInsideBulbs(number.ToDouble());

    static bool IsInLargerBulb(Complex number)
    {
        var realMinusFourth = number.Real - 0.25;
        var imagSquared = number.Imaginary * number.Imaginary;
        var q = realMinusFourth * realMinusFourth + imagSquared;

        return (q * (q + realMinusFourth)) < (0.25 * imagSquared);
    }

    static bool IsInCircularBulbs(Complex number)
    {
        return
            IsInsideCircle(new Complex(-1, 0), 0.25, number) ||
            IsInsideCircle(new Complex(-0.125, 0.744), 0.092, number) ||
            // Ignore stuff on the negative imaginary side
            //IsInsideCircle(new Complex(-0.125, -0.744), 0.092, number) ||
            IsInsideCircle(new Complex(-1.308, 0), 0.058, number);
    }

    static bool IsInsideCircle(Complex center, double radius, Complex number)
    {
        var translated = number - center;

        return translated.MagnitudeSquared() <= (radius * radius);
    }
}