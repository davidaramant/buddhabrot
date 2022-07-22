using System.Drawing;
using BenchmarkDotNet.Attributes;
using Buddhabrot.Core;
using Buddhabrot.Extensions;
using Buddhabrot.IterationKernels;

namespace Buddhabrot.Benchmarks;

public class AutomaticIterationLimitBenchmarks
{
    private const int MaxIterations = 10000;
    private readonly ViewPort _viewPort = new(Constant.RenderingArea.GetPositiveImagArea(), new Size(100, 100).HalveVertically());

    [Benchmark]
    public int FiniteIterationLimit()
    {
        return
            _viewPort.Resolution.GetPositionsRowFirst().
                AsParallel().
                Select(p => ScalarDoubleKernel.FindEscapeTimeNoCycleDetection(_viewPort.GetComplex(p), MaxIterations).IsInfinite).
                Count();
    }

    [Benchmark]
    public int CycleDetection()
    {
        return
            _viewPort.Resolution.GetPositionsRowFirst().
                AsParallel().
                Select(p => ScalarDoubleKernel.FindEscapeTime(_viewPort.GetComplex(p)).IsInfinite).
                Count();
    }

    [Benchmark]
    public int BothCycleDetectionAndFiniteLimit()
    {
        return
            _viewPort.Resolution.GetPositionsRowFirst().
                AsParallel().
                Select(p => ScalarDoubleKernel.FindEscapeTime(_viewPort.GetComplex(p), MaxIterations).IsInfinite).
                Count();
    }
}