using System.Numerics;
using Buddhabrot.Core;
using NUnit.Framework;

namespace Tests.Core
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
            var pair = new PointPair(new Complex(aR, aI), new Complex(bR, bI));

            var middle = pair.GetMidPoint();

            Assert.That(middle.Real, Is.EqualTo(expectedR).Within(0.00000001f), "Incorrect real component.");
            Assert.That(middle.Imaginary, Is.EqualTo(expectedI).Within(0.00000001f), "Incorrect imaginary component.");
        }
    }
}
