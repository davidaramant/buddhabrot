using Buddhabrot.Core;

namespace Buddhabrot.Edges
{
    public struct PointPair
    {
        private readonly FComplex InSet;
        private readonly FComplex NotInSet;

        public PointPair(FComplex inSet, FComplex notInSet)
        {
            InSet = inSet;
            NotInSet = notInSet;
        }

        public FComplex GetMidPoint()
        {
            return new FComplex(
                MidPointOf(InSet.Real, NotInSet.Real),
                MidPointOf(InSet.Imag, NotInSet.Imag));
        }

        private static float MidPointOf(float a, float b)
        {
            return a + 0.5f * (b - a);
        }
    }
}
