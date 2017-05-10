using System.Numerics;
using Buddhabrot.Core;
using NUnit.Framework;

namespace Tests.Core
{
    [TestFixture]
    public sealed class EdgeSpanTests
    {
        [TestCase(1d, 0d, 0d, 0d, 0.5d, 0d)]
        [TestCase(0d, 0d, 1d, 0d, 0.5d, 0d)]
        [TestCase(0d, 0d, 1d, 1d, 0.5d, 0.5d)]
        [TestCase(1d, 1d, 0d, 0d, 0.5d, 0.5d)]
        public void ShouldFindMiddle(double aR, double aI, double bR, double bI, double expectedR, double expectedI)
        {
            var span = new EdgeSpan(new Complex(aR, aI), new Complex(bR, bI));

            var middle = span.GetMidPoint();

            Assert.That(middle.Real, Is.EqualTo(expectedR).Within(0.00000001f), "Incorrect real component.");
            Assert.That(middle.Imaginary, Is.EqualTo(expectedI).Within(0.00000001f), "Incorrect imaginary component.");
        }
    }
}
