using System.Drawing;
using System.Linq;
using Buddhabrot.Core;
using Buddhabrot.EdgeSpans;
using Buddhabrot.IterationKernels;
using NUnit.Framework;

namespace Tests.EdgeSpans;

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
                viewPort: new ViewPort(
                    new ComplexArea(new DoubleRange(-1, 1), new DoubleRange(-1, 1)),
                    new Size(3, 3)),
                computationType: ComputationType.ScalarDouble,
                spans: new[]
                {
                    new LogicalEdgeSpan(new Point(1, 1), ToOutside: Direction.Up),
                });

            using (var stream = EdgeSpanStream.Load(tempFile.Path))
            {
                var spans = stream.ToArray();

                Assert.That(spans, Has.Length.EqualTo(1), "Incorrect number of spans.");

                var span = spans.First();
                Assert.That(span.Location, Is.EqualTo(new Point(1, 1)), "Wrong location.");
                Assert.That(span.ToOutside, Is.EqualTo(Direction.Up), "Wrong direction.");
            }
        }
    }
}