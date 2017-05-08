using Buddhabrot.Core;
using Buddhabrot.Edges;
using NUnit.Framework;

namespace Tests.Edges
{
    [TestFixture]
    public sealed class PointPairTests
    {
        [TestCase(1f, 0f, 0f, 0f, 0.5f, 0f)]
        [TestCase(0f, 0f, 1f, 0f, 0.5f, 0f)]
        [TestCase(0f, 0f, 1f, 1f, 0.5f, 0.5f)]
        [TestCase(1f, 1f, 0f, 0f, 0.5f, 0.5f)]
        public void ShouldFindMiddle(float aR, float aI, float bR, float bI, float expectedR, float expectedI)
        {
            var pair = new PointPair(new FComplex(aR, aI), new FComplex(bR, bI));

            var middle = pair.GetMidPoint();

            Assert.That(middle.Real, Is.EqualTo(expectedR).Within(0.00000001f), "Incorrect real component.");
            Assert.That(middle.Imag, Is.EqualTo(expectedI).Within(0.00000001f), "Incorrect imaginary component.");
        }
    }
}
