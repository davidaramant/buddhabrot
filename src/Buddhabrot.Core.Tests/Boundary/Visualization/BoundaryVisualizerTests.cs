using Buddhabrot.Core.Boundary.Visualization;
using Buddhabrot.Core.Boundary;
using SkiaSharp;

namespace Buddhabrot.Core.Tests.Boundary.Visualization;

public class BoundaryVisualizerTests
{
    [Fact]
    public void ShouldProduceSameOutput()
    {
        var power3Regions = new[]
        {
            (0, 0), (1, 0), (2, 0), (3, 0), (3, 1), (4, 1), (4, 0), (5, 0), (5, 1), (5, 2), (6, 2), (7, 2),
            (8, 2), (9, 2), (9, 1), (9, 0)
        }
        .Select(t => (new RegionId(t.Item1, t.Item2), RegionType.Border)).ToList();

        var lookup = new RegionLookup(new AreaDivisions(3), power3Regions);

        using var actualRasterImage = BoundaryVisualizer.RenderRegionLookup(lookup);
        var actualImage = actualRasterImage.Raw;
        using var expectedImage = ReadEmbeddedImage("Power3Quadtree.png");

        using var actualPixels = actualImage.Encode(SKEncodedImageFormat.Png, 100);
        using var expectedPixels = expectedImage.Encode(SKEncodedImageFormat.Png, 100);

        var actualBytes = actualPixels.ToArray();
        var expectedBytes = expectedPixels.ToArray();

        actualBytes.Should().BeEquivalentTo(expectedBytes);
    }

    private static SKBitmap ReadEmbeddedImage(string fileName)
    {
        var testType = typeof(BoundaryVisualizerTests);
        var assembly = testType.Assembly;
        var ns = testType.Namespace;
        using var stream = assembly.GetManifestResourceStream(ns + "." + fileName);
        using var managedStream = new SKManagedStream(stream);
        return SKBitmap.Decode(managedStream);
    }
}
