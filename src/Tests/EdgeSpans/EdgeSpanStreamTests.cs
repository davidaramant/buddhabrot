using System.Drawing;
using System.Linq;
using System.Numerics;
using Buddhabrot.Core;
using Buddhabrot.EdgeSpans;
using NUnit.Framework;

namespace Tests.EdgeSpans
{
    [TestFixture]
    public sealed class EdgeSpanStreamTests
    {
        [Test]
        public void ShouldRoundTrip()
        {
            using (var tempFile = new TempFile())
            {
                EdgeSpanStream.Write(
                    tempFile.Path,
                    pointResolution: new Size(3, 3),
                    viewPort: new ComplexArea(new DoubleRange(-1, 1), new DoubleRange(-1, 1)),
                    spans: new[]
                    {
                        new LogicalEdgeSpan(1,1,Direction.Up),
                    });

                using (var stream = EdgeSpanStream.Load(tempFile.Path))
                {
                    var spans = stream.ToArray();

                    Assert.That(spans, Has.Length.EqualTo(1), "Incorrect number of spans.");

                    var span = spans.First().span;
                    Assert.That(span.InSet, Is.EqualTo(new Complex(0, 0)), "Wrong point in the set.");
                    Assert.That(span.NotInSet, Is.EqualTo(new Complex(0, 1)), "Wrong point in the set.");
                }
            }

        }
    }
}
