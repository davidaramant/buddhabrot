using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using Buddhabrot.Core;
using Buddhabrot.Edges;
using NUnit.Framework;

namespace Tests.Edges
{
    [TestFixture]
    public sealed class EdgeSpanStreamTests
    {
        [Test]
        public void ShouldRoundTrip()
        {
            var filePath = Path.GetTempFileName();
            try
            {
                EdgeSpanStream.Write(
                    filePath,
                    pointResolution: new Size(3, 3),
                    viewPort: new ComplexArea(new DoubleRange(-1, 1), new DoubleRange(-1, 1)),
                    spans: new[]
                    {
                        new LogicalEdgeSpan(1,1,Direction.Up),
                    });

                using (var stream = EdgeSpanStream.Load(filePath))
                {
                    var spans = stream.ToArray();

                    Assert.That(spans, Has.Length.EqualTo(1), "Incorrect number of spans.");

                    var span = spans.First();
                    Assert.That(span.InSet, Is.EqualTo(new Complex(0, 0)), "Wrong point in the set.");
                    Assert.That(span.NotInSet, Is.EqualTo(new Complex(0, 1)), "Wrong point in the set.");
                }
            }
            finally
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }
    }
}
