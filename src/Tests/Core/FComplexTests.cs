using Buddhabrot.Core;
using NUnit.Framework;

namespace Tests.Core
{
    [TestFixture]
    public sealed class FComplexTests
    {
        [TestCase(1f, 0f, 0f, 0f, 0.5f, 0f)]
        [TestCase(0f, 0f, 1f, 0f, 0.5f, 0f)]
        [TestCase(0f, 0f, 1f, 1f, 0.5f, 0.5f)]
        [TestCase(1f, 1f, 0f, 0f, 0.5f, 0.5f)]
        public void ShouldFindMiddle(float aR, float aI, float bR, float bI, float expectedR, float expectedI)
        {
            var a = new FComplex(aR, aI);
            var b = new FComplex(bR, bI);

            var middle = FComplex.MidPointOf(a, b);

            Assert.That(middle.Real, Is.EqualTo(expectedR).Within(0.00000001f), "Incorrect real component.");
            Assert.That(middle.Imag, Is.EqualTo(expectedI).Within(0.00000001f), "Incorrect imaginary component.");
        }
    }
}
