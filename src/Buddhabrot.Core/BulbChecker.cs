using System.Numerics;

namespace Buddhabrot.Core;

public static class BulbChecker
{
	/// <summary>
	/// Does a fast check to see if a complex number lies within one of the larger bulbs of the Mandelbrot set.
	/// </summary>
	public static bool IsInsideBulbs(double real, double imag) => IsInsideBulbs(new Complex(real, imag));

	/// <summary>
	/// Does a fast check to see if a complex number lies within one of the larger bulbs of the Mandelbrot set.
	/// </summary>
	public static bool IsInsideBulbs(Complex number) => IsInMainCardioid(number) || IsInCircle(number);

	static bool IsInMainCardioid(Complex number)
	{
		var realMinusFourth = number.Real - 0.25;
		var imagSquared = number.Imaginary * number.Imaginary;
		var q = realMinusFourth * realMinusFourth + imagSquared;

		return (q * (q + realMinusFourth)) < (0.25 * imagSquared);
	}

	static bool IsInCircle(Complex number)
	{
		var translated = number - new Complex(-1, 0);

		return translated.MagnitudeSquared() <= (0.25 * 0.25);
	}
}
