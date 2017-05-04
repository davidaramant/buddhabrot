using Buddhabrot.Core;

namespace Buddhabrot
{
    static class Constant
    {
        public static readonly IntRange IterationRange = new IntRange(20_000_000, 30_000_000);
        public static readonly ComplexArea RenderingArea = new ComplexArea(
            realRange: new FloatRange(-1.45f, 0.75f),
            imagRange: new FloatRange(-1.1f, 1.1f));
    }
}
