using System.Drawing;
using System.Numerics;
using Buddhabrot.Core;
using NUnit.Framework;

namespace Tests.Core
{
    [TestFixture]
    public sealed class ViewPortTests
    {
        [Test]
        public void ShouldFigureOutMiddleOfSquareArea()
        {
            var viewPort = new ViewPort(
                new ComplexArea(
                    new DoubleRange(-1, 1),
                    new DoubleRange(-1, 1)),
                new Size(101, 101));

            var middle = viewPort.GetPosition(new Complex());

            Assert.That(middle, Is.EqualTo(new Point(50, 50)), "Incorrect position.");
        }

        [Test]
        public void ShouldUseTopLeftAsPositionOrigin()
        {
            var viewPort = new ViewPort(
                new ComplexArea(
                    new DoubleRange(-1, 1),
                    new DoubleRange(-1, 1)),
                new Size(101, 101));

            var topLeft = viewPort.GetPosition(new Complex(-1, 1));

            Assert.That(topLeft, Is.EqualTo(new Point(0, 0)), "Incorrect position.");
        }

        [Test]
        public void ShouldRoundTripPositions()
        {
            var viewPort = new ViewPort(
                new ComplexArea(
                    new DoubleRange(-1, 1),
                    new DoubleRange(-1, 1)),
                new Size(101, 101));

            var c = new Complex(-1, 1);

            var roundTripped = viewPort.GetComplex(viewPort.GetPosition(c));
            Assert.That(roundTripped, Is.EqualTo(c), "Returned wrong complex number.");
        }

        [Test]
        public void ShouldRoundTripComplexNumbers()
        {
            var viewPort = new ViewPort(
                new ComplexArea(
                    new DoubleRange(-1, 1),
                    new DoubleRange(-1, 1)),
                new Size(101, 101));

            var point = new Point(-1, 1);

            var roundTripped = viewPort.GetPosition(viewPort.GetComplex(point));
            Assert.That(roundTripped, Is.EqualTo(point), "Returned wrong position.");
        }
    }
}
