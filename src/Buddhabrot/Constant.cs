using Buddhabrot.Core;

namespace Buddhabrot
{
    static class Constant
    {
        // The upper range is hard-coded in the OpenCL kernel
        public static readonly IntRange IterationRange = new IntRange(20_000_000, 30_000_000);
        public static readonly ComplexArea RenderingArea = new ComplexArea(
            realRange: new DoubleRange(-1.45f, 0.75f),
            imagRange: new DoubleRange(-1.1f, 1.1f));
    }
}
