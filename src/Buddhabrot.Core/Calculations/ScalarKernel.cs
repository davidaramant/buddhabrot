using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace Buddhabrot.Core.Calculations;

[SuppressMessage(
	"ReSharper",
	"CompareOfFloatsByEqualityOperator",
	Justification = "Cycle detection requires direct float comparisons."
)]
public static class ScalarKernel
{
	public static EscapeTime FindEscapeTime(Complex c, int maxIterations)
	{
		// Uses Brent's cycle detection algorithm: https://en.wikipedia.org/wiki/Cycle_detection#Brent's_algorithm
		// Using "old & older" is a modification I made. Not sure if this is optimal, but it makes a slight improvement
		// in benchmarks.

		if (BulbChecker.IsInsideBulbs(c))
			return EscapeTime.Infinite;

		var zReal = 0.0;
		var zImag = 0.0;

		var z2Real = 0.0;
		var z2Imag = 0.0;

		var olderZReal = 0.0;
		var olderZImag = 0.0;

		var oldZReal = 0.0;
		var oldZImag = 0.0;

		int stepsTaken = 0;
		int stepLimit = 2;

		for (int i = 0; i < maxIterations; i++)
		{
			stepsTaken++;

			zImag = 2 * zReal * zImag + c.Imaginary;
			zReal = z2Real - z2Imag + c.Real;

			if ((oldZReal == zReal && oldZImag == zImag) || (olderZReal == zReal && olderZImag == zImag))
				return EscapeTime.Infinite;

			z2Real = zReal * zReal;
			z2Imag = zImag * zImag;

			if ((z2Real + z2Imag) > 4)
			{
				return new EscapeTime(i);
			}

			if (stepsTaken == stepLimit)
			{
				olderZReal = oldZReal;
				olderZImag = oldZImag;
				oldZReal = zReal;
				oldZImag = zImag;
				stepsTaken = 0;
				stepLimit <<= 1;
			}
		}

		return EscapeTime.Infinite;
	}

	public static (EscapeTime Iterations, double Distance) FindExteriorDistance(Complex c, int maxIterations)
	{
		// http://mrob.com/pub/muency/distanceestimator.html

		if (BulbChecker.IsInsideBulbs(c))
			return (EscapeTime.Infinite, 0);

		var zReal = 0.0;
		var zImag = 0.0;

		var z2Real = 0.0;
		var z2Imag = 0.0;

		var oldZReal = 0.0;
		var oldZImag = 0.0;

		var olderZReal = 0.0;
		var olderZImag = 0.0;

		var dZReal = 0.0;
		var dZImag = 0.0;

		int stepsTaken = 0;
		int stepLimit = 2;

		for (int i = 0; i < maxIterations; i++)
		{
			stepsTaken++;

			//dZ = 2 * z * dZ + 1;
			var temp = 2 * (zReal * dZReal - zImag * dZImag) + 1;
			dZImag = 2 * (zReal * dZImag + zImag * dZReal);
			dZReal = temp;

			zImag = 2 * zReal * zImag + c.Imaginary;
			zReal = z2Real - z2Imag + c.Real;

			if ((oldZReal == zReal && oldZImag == zImag) || (olderZReal == zReal && olderZImag == zImag))
				return (EscapeTime.Infinite, 0);

			z2Real = zReal * zReal;
			z2Imag = zImag * zImag;

			if ((z2Real + z2Imag) > 4)
			{
				var magZ = Hypot(zReal, zImag);
				var magDZ = Hypot(dZReal, dZImag);
				return (new EscapeTime(i), Math.Log(magZ * magZ) * magZ / magDZ);
			}

			if (stepsTaken == stepLimit)
			{
				olderZReal = oldZReal;
				olderZImag = oldZImag;
				oldZReal = zReal;
				oldZImag = zImag;
				stepsTaken = 0;
				stepLimit <<= 1;
			}
		}

		// Match the normal escape time algorithm and treat falling out of the loop as being in the set
		return (EscapeTime.Infinite, 0);
	}

	private static double Hypot(double a, double b)
	{
		// Using
		//   sqrt(a^2 + b^2) = |a| * sqrt(1 + (b/a)^2)
		// we can factor out the larger component to dodge overflow even when a * a would overflow.

		a = Math.Abs(a);
		b = Math.Abs(b);

		double small,
			large;
		if (a < b)
		{
			small = a;
			large = b;
		}
		else
		{
			small = b;
			large = a;
		}

		if (small == 0.0)
		{
			return (large);
		}
		else if (double.IsPositiveInfinity(large) && !double.IsNaN(small))
		{
			// The NaN test is necessary so we don't return +inf when small=NaN and large=+inf.
			// NaN in any other place returns NaN without any special handling.
			return (double.PositiveInfinity);
		}
		else
		{
			double ratio = small / large;
			return (large * Math.Sqrt(1.0 + ratio * ratio));
		}
	}
}
