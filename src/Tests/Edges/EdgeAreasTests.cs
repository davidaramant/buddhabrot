using System.Drawing;
using System.IO;
using Buddhabrot.Core;
using Buddhabrot.Edges;
using NUnit.Framework;

namespace Tests.Edges
{
    [TestFixture]
    public sealed class EdgeAreasTests
    {
        [Test]
        public void ShouldRoundTripAreasToDisk()
        {
            var filePath = Path.GetTempFileName();
            try
            {
                ComplexArea CreateArea(int value) => new ComplexArea(new Range(value, value + 1), new Range(value + 2, value + 3));

                var size = new Size(1, 2);
                var viewPort = CreateArea(1);
                var edgeAreas = new[]
                {
                    CreateArea(10),
                    CreateArea(20),
                };

                EdgeAreas.Write(filePath, size, viewPort, edgeAreas);

                var roundTripped = EdgeAreas.Load(filePath);

                void RangesAreEqual(Range actual, Range expected, string name)
                {
                    Assert.That(actual.InclusiveMin, Is.EqualTo(expected.InclusiveMin).Within(0.0000001f), $"{name} range minimum was different.");
                    Assert.That(actual.ExclusiveMax, Is.EqualTo(expected.ExclusiveMax).Within(0.0000001f), $"{name} range maximum was different.");
                }

                void AreasAreEqual(ComplexArea actual, ComplexArea expected)
                {
                    RangesAreEqual(actual.RealRange, expected.RealRange, "Real");
                    RangesAreEqual(actual.ImagRange, expected.ImagRange, "Imag");
                }

                Assert.That(roundTripped.GridResolution, Is.EqualTo(size), "Did not round-trip grid resolution.");
                AreasAreEqual(roundTripped.ViewPort, viewPort);
                // If the view port was saved/loaded correctly, the edges are too.  Just check the number.
                Assert.That(roundTripped.AreaCount,Is.EqualTo(edgeAreas.Length),"Did not round-trip the edge areas");
            }
            finally
            {
                File.Delete(filePath);
            }
        }
    }
}
