using System.Drawing;
using System.IO;
using System.Linq;
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
                EdgeArea CreateEdgeArea(int x, int y) => new EdgeArea(new Point(x, y));

                var size = new Size(1, 2);
                var viewPort = CreateArea(1);
                var edgeAreas = new[]
                {
                    CreateEdgeArea(0,1),
                    CreateEdgeArea(2,3),
                };

                var areas = EdgeAreas.CreateCompressed(size, viewPort, edgeAreas);
                areas.Write(filePath);

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
                Assert.That(roundTripped.AreaCount, Is.EqualTo(edgeAreas.Length), "Did not round-trip the edge areas");
                Assert.That(roundTripped.GetAreaLocations().ToArray(), Is.EqualTo(edgeAreas.Select(ea => ea.GridLocation).ToArray()),
                    "Did not round-trip the area locations.");
            }
            finally
            {
                File.Delete(filePath);
            }
        }
    }
}
