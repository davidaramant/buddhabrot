namespace Buddhabrot.Core
{
    public static class MandelbulbChecker
    {
        /// <summary>
        /// Does a fast check to see if a complex number lies within one of the larger bulbs of the Mandelbrot set.
        /// </summary>
        public static bool IsInsideBulbs(FComplex number)
        {
            return
                IsInLargerBulb(number) ||
                IsInCircularBulbs(number);
        }

        static bool IsInLargerBulb(FComplex number)
        {
            var realMinusFourth = number.Real - 0.25;
            var q = realMinusFourth * realMinusFourth + number.Imag * number.Imag;

            return (q * (q + (number.Real - 0.25))) < (0.25 * number.Imag * number.Imag);
        }

        static bool IsInCircularBulbs(FComplex number)
        {
            return
                IsInsideCircle(new FComplex(-1f, 0f), 0.25f, number) ||
                IsInsideCircle(new FComplex(-0.125f, 0.744f), 0.092f, number) ||
                IsInsideCircle(new FComplex(-0.125f, -0.744f), 0.092f, number) ||
                IsInsideCircle(new FComplex(-1.308f, 0f), 0.058f, number);
        }

        static bool IsInsideCircle(FComplex center, float radius, FComplex number)
        {
            var translated = new FComplex(
                real: number.Real - center.Real, 
                imag: number.Imag - center.Imag);

            return (translated.Real * translated.Real + translated.Imag * translated.Imag) <= (radius * radius);
        }
    }
}
