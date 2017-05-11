using System.Drawing;
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
            using (var tempFile = new TempFile())
            {
                ComplexArea CreateArea(int value) => new ComplexArea(new DoubleRange(value, value + 1), new DoubleRange(value + 2, value + 3));
                EdgeArea CreateEdgeArea(int x, int y) => new EdgeArea(new Point(x, y), Corners.BottomRight);

                var size = new Size(1, 2);
                var viewPort = CreateArea(1);
                var edgeAreas = new[]
                {
                    CreateEdgeArea(0,1),
                    CreateEdgeArea(2,3),
                };

                EdgeAreas.Write(size, viewPort, edgeAreas, tempFile.Path);

                using (var roundTripped = EdgeAreas.Load(tempFile.Path))
                {
                    void RangesAreEqual(DoubleRange actual, DoubleRange expected, string name)
                    {
                        Assert.That(actual.InclusiveMin, Is.EqualTo(expected.InclusiveMin).Within(0.0000001f),
                            $"{name} range minimum was different.");
                        Assert.That(actual.ExclusiveMax, Is.EqualTo(expected.ExclusiveMax).Within(0.0000001f),
                            $"{name} range maximum was different.");
                    }

                    void AreasAreEqual(ComplexArea actual, ComplexArea expected)
                    {
                        RangesAreEqual(actual.RealRange, expected.RealRange, "Real");
                        RangesAreEqual(actual.ImagRange, expected.ImagRange, "Imag");
                    }

                    Assert.That(roundTripped.GridResolution, Is.EqualTo(size), "Did not round-trip grid resolution.");
                    AreasAreEqual(roundTripped.ViewPort, viewPort);
                    Assert.That(roundTripped.AreaCount, Is.EqualTo(edgeAreas.Length),
                        "Did not round-trip the edge areas");
                    Assert.That(roundTripped.GetAreaLocations().ToArray(),
                        Is.EqualTo(edgeAreas.Select(ea => ea.GridLocation).ToArray()),
                        "Did not round-trip the area locations.");
                }
            }
        }
    }
}
