using System.Drawing;
using Buddhabrot.Core;

namespace Buddhabrot
{
    static class Constant
    {
        public static readonly IterationRange IterationRange = new IterationRange(20_000_000, 30_000_000);
        public static readonly ComplexArea RenderingArea = new ComplexArea(
            realRange: new Range(-1.45f, 0.75f),
            imagRange: new Range(-1.1f, 1.1f));
        public static readonly Size EdgeGridResolution = new Size(16384, 16384);
    }
}
