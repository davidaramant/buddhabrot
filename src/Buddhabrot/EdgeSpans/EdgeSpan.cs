using System;
using System.Numerics;
using Buddhabrot.Extensions;
using Buddhabrot.IterationKernels;

namespace Buddhabrot.EdgeSpans
{
    /// <summary>
    /// A line segment that spans across the set boundary.
    /// </summary>
    public struct EdgeSpan
    {
        public readonly Complex InSet;
        public readonly Complex NotInSet;

        public EdgeSpan(Complex inSet, Complex notInSet)
        {
            InSet = inSet;
            NotInSet = notInSet;
        }

        public double Length() => (InSet - NotInSet).Magnitude;

        public double LengthSquared() => (InSet - NotInSet).MagnitudeSquared();

        public Complex GetMidPoint()
        {
            return new Complex(
                MidPointOf(InSet.Real, NotInSet.Real),
                MidPointOf(InSet.Imaginary, NotInSet.Imaginary));
        }

        private static double MidPointOf(double a, double b)
        {
            return a + 0.5 * (b - a);
        }

        public override string ToString() => $"(In Set: {InSet}, Not in Set: {NotInSet})";

        public Complex FindBoundaryPoint(int iterationlimit) => FindBoundaryPoint(this, iterationlimit);

        public static Complex FindBoundaryPoint(EdgeSpan span, int iterationLimit)
        {
            double lastLength = 0;
            while (true)
            {
                var length = span.Length();
                if (length == lastLength)
                {
                    return span.NotInSet;
                }
                lastLength = length;
                var middle = span.GetMidPoint();

                var escapeTime = ScalarDoubleKernel.FindEscapeTime(middle, iterationLimit);

                span = escapeTime.IsInfinite
                    ? new EdgeSpan(middle, span.NotInSet)
                    : new EdgeSpan(span.InSet, middle);
            }
        }
    }
}
