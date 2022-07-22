using System.Numerics;
using BenchmarkDotNet.Attributes;
using Buddhabrot.EdgeSpans;
using Buddhabrot.IterationKernels;

namespace Buddhabrot.Benchmarks;

public class BorderPointBenchmarks
{
    public static readonly EdgeSpan Span = new(
        inSet: new Complex(0.326119203973466, 0.574384958997266),
        notInSet: new Complex(0.326119203973466, 0.574458297219815));

    [Benchmark(Baseline = true)]
    public Complex FindBorderPoint()
    {
        return Span.FindBoundaryPoint(Constant.IterationRange.Max);
    }

    [Benchmark]
    public Complex FindBorderPointsMagnitudeSquared()
    {
        return FindBorderPointMagnitudeSquared(Span, Constant.IterationRange.Max);
    }

    static Complex FindBorderPointMagnitudeSquared(EdgeSpan span, int iterationLimit)
    {
//            var initialLength2 = span.LengthSquared();
        int maxSubdivisions = 38;
        //double oldLength = 0;
        //while (oldLength != initialLength2)
        //{
        //    maxSubdivisions++;
        //    oldLength = initialLength2;
        //    initialLength2 /= 2;
        //}

        //Console.WriteLine(maxSubdivisions);

        double lastLength = 0;

        for (int i = 0; i < maxSubdivisions; i++)
        {
            //var length = span.LengthSquared();
            //if (length == lastLength)
            //{
            //    return span.NotInSet;
            //}
            //lastLength = length;
            var middle = span.GetMidPoint();

            var escapeTime = ScalarDoubleKernel.FindEscapeTime(middle, iterationLimit);

            span = escapeTime.IsInfinite
                ? new EdgeSpan(middle, span.NotInSet)
                : new EdgeSpan(span.InSet, middle);

            //if (i % 10 == 0)
            //{
            //    Console.WriteLine(i);
            //}
        }

        return span.NotInSet;
    }

    static Complex[] FindBorderPoints(EdgeSpan[] spans, int iterationLimit)
    {
        return null;
    }

    public struct VectorComplex
    {
        public readonly Vector<double> Real;
        public readonly Vector<double> Imag;


    }
}