using System.Drawing;
using Buddhabrot.Core;
using Buddhabrot.Edges;
using NUnit.Framework;

namespace Tests.Edges
{
    [TestFixture]
    public sealed class CornerCalculatorTests
    {
        [Test]
        public void ShouldCalculateCornersOfUnitGrid()
        {
            var calculator = new CornerCalculator(
                gridSize: new Size(1, 1),
                viewPort: new ComplexArea(realRange: new DoubleRange(-1, 1), imagRange: new DoubleRange(-1, 1)));

            AssertCorner(calculator, new Point(), Corners.BottomLeft, -1, -1);
            AssertCorner(calculator, new Point(), Corners.TopLeft, -1, 1);
            AssertCorner(calculator, new Point(), Corners.BottomRight, 1, -1);
            AssertCorner(calculator, new Point(), Corners.TopRight, 1, 1);
        }

        [Test]
        public void ShouldCalculateCornersOf2X2Grid()
        {
            var calculator = new CornerCalculator(
                gridSize: new Size(2, 2),
                viewPort: new ComplexArea(realRange: new DoubleRange(-1, 1), imagRange: new DoubleRange(-1, 1)));

            AssertCorner(calculator, new Point(), Corners.BottomLeft, -1, -1);
            AssertCorner(calculator, new Point(), Corners.TopLeft, -1, 0);
            AssertCorner(calculator, new Point(), Corners.BottomRight, 0, -1);
            AssertCorner(calculator, new Point(), Corners.TopRight, 0, 0);
        }

        static void AssertCorner(CornerCalculator calculator, Point location, Corners corner, double real, double imag)
        {
            var point = calculator.GetCorner(location, corner);
            Assert.That(point.Real, Is.EqualTo(real), "Incorrect real value");
            Assert.That(point.Imaginary, Is.EqualTo(imag), "Incorrect imaginary value.");
        }
    }
}
