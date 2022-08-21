using System.Numerics;
using Buddhabrot.Core;
using Buddhabrot.IterationKernels;

namespace Buddhabrot.EdgeSpans;

/// <summary>
/// A line segment that spans across the set boundary.
/// </summary>
public readonly record struct EdgeSpan(
    Complex InSet,
    Complex NotInSet)
{
    public double Length() => (InSet - NotInSet).Magnitude;

    public double LengthSquared() => (InSet - NotInSet).MagnitudeSquared();

    public Complex GetMidPoint() =>
        new(
            MidPointOf(InSet.Real, NotInSet.Real),
            MidPointOf(InSet.Imaginary, NotInSet.Imaginary));

    private static double MidPointOf(double a, double b) => a + 0.5 * (b - a);

    public override string ToString() => $"(In Set: {InSet}, Not in Set: {NotInSet})";

    public Complex FindBoundaryPoint() => FindBoundaryPoint(this);

    public static Complex FindBoundaryPoint(EdgeSpan span)
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

            var escapeTime = ScalarDoubleKernel.FindEscapeTime(middle);

            span = escapeTime.IsInfinite
                ? new EdgeSpan(middle, span.NotInSet)
                : new EdgeSpan(span.InSet, middle);
        }
    }


    public Complex FindBoundaryPoint(int iterationLimit) => FindBoundaryPoint(this, iterationLimit);

    public static Complex FindBoundaryPoint(EdgeSpan span, int iterationLimit)
    {
        double lastLength = 0;
        const int bailout = 45;
        int index = 0;
        while (true)
        {
            var length = span.Length();
            if (length == lastLength || index == bailout)
            {
                return span.NotInSet;
            }

            lastLength = length;
            var middle = span.GetMidPoint();

            var escapeTime = ScalarDoubleKernel.FindEscapeTime(middle, iterationLimit);

            span = escapeTime.IsInfinite
                ? new EdgeSpan(middle, span.NotInSet)
                : new EdgeSpan(span.InSet, middle);

            index++;
        }
    }
}