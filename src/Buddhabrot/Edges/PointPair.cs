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
    }
}
