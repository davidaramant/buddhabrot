using System.Numerics;

namespace Buddhabrot.Edges
{
    public struct PointPair
    {
        public readonly Complex InSet;
        public readonly Complex NotInSet;

        public PointPair(Complex inSet, Complex notInSet)
        {
            InSet = inSet;
            NotInSet = notInSet;
        }

        public Complex GetMidPoint()
        {
            return new Complex(
                MidPointOf(InSet.Real, NotInSet.Real),
                MidPointOf(InSet.Imaginary, NotInSet.Imaginary));
        }

        private static double MidPointOf(double a, double b)
        {
            return a + 0.5 * (b - a);
        }

        public override string ToString() => $"(In Set: {InSet}, Not in Set: {NotInSet})";
    }
}
