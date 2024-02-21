using Buddhabrot.Core.Boundary.Visualization;
using Buddhabrot.Core.Tests.Boundary.QuadTrees;
using SkiaSharp;

namespace Buddhabrot.Core.Tests.Boundary.Visualization;

public class BoundaryVisualizerTests
{
    [Fact]
    public void ShouldProduceSameOutput()
    {
        var lookup = RegionLookupUtil.Power3Lookup;

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
