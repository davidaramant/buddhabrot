using Buddhabrot.Core;

namespace Buddhabrot;

public static class Constant
{
    // The upper range is hard-coded in the OpenCL kernel
    public static readonly IterationRange IterationRange = new IterationRange(10_000_000, 15_000_000);
    public static readonly ComplexArea RenderingArea = new ComplexArea(
        realRange: new DoubleRange(-1.45, 0.75),
        imagRange: new DoubleRange(-1.1, 1.1));
}