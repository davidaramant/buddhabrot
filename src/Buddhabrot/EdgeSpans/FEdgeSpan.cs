using Buddhabrot.Core;
using Buddhabrot.IterationKernels;

namespace Buddhabrot.EdgeSpans;

/// <summary>
/// A line segment that spans across the set boundary.
/// </summary>
public readonly record struct FEdgeSpan(
    FComplex InSet,
    FComplex NotInSet)
{
    public double Length() => (InSet - NotInSet).Magnitude;

    public double LengthSquared() => (InSet - NotInSet).MagnitudeSquared();

    public FComplex GetMidPoint() =>
        new (
            MidPointOf(InSet.Real, NotInSet.Real),
            MidPointOf(InSet.Imaginary, NotInSet.Imaginary));

    private static float MidPointOf(float a, float b) => (a - a / 2) + b / 2;

    public override string ToString() => $"(In Set: {InSet}, Not in Set: {NotInSet})";


    public FComplex FindBoundaryPoint(int iterationLimit) => FindBoundaryPoint(this, iterationLimit);

    public static FComplex FindBoundaryPoint(FEdgeSpan span, int iterationLimit)
    {
        double lastLength = 0;
        const int bailout = 20;
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

            var escapeTime = ScalarFloatKernel.FindEscapeTime(middle, iterationLimit);

            span = escapeTime.IsInfinite
                ? new FEdgeSpan(middle, span.NotInSet)
                : new FEdgeSpan(span.InSet, middle);

            index++;
        }
    }
}