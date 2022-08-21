using System.Drawing;
using Buddhabrot.Core;
using Buddhabrot.Core.Images;
using Buddhabrot.Core.IterationKernels;

namespace Buddhabrot.ManualVisualizations;

[Explicit]
public class FilamentVisualizations : BaseVisualization
{
    [OneTimeSetUp]
    public void CreateOutputPath() => SetUpOutputPath(nameof(FilamentVisualizations));

    [Test]
    public void ShouldCalculateFilaments()
    {
        var viewPort = new ViewPort(
            new ComplexArea(
                new Interval(-2, 2),
                new Interval(0, 2)),
            new Size(1000, 500));

        var escapeLimitsThousands = new[] { 1, 5, 10 };

        using var image = new RasterImage(viewPort.Resolution);

        foreach (var maxK in escapeLimitsThousands)
        {
            RenderEscapeTimeSet(viewPort, maxK * 1000, image);

            SaveImage(image, $"Escape Time {maxK}K");

            RenderFilaments(viewPort, maxK * 1000, image);

            SaveImage(image, $"Distance Estimate {maxK}K");
        }
    }

    private static void RenderEscapeTimeSet(ViewPort viewPort, int max, RasterImage image)
    {
        Parallel.For(0, viewPort.Resolution.Height, row =>
        {
            for (int col = 0; col < viewPort.Resolution.Width; col++)
            {
                var inSet = ScalarKernel.FindEscapeTime(viewPort.GetComplex(col, row), max).IsInfinite;

                image.SetPixel(col, row, inSet ? Color.DarkBlue : Color.White);
            }
        });
    }

    private static void RenderFilaments(ViewPort viewPort, int max, RasterImage image)
    {
        Parallel.For(0, viewPort.Resolution.Height, row =>
        {
            for (int col = 0; col < viewPort.Resolution.Width; col++)
            {
                var distance =
                    ScalarKernel.FindExteriorDistance(viewPort.GetComplex(col, row), max);

                var color = distance switch
                {
                    Double.MaxValue => Color.DarkBlue,
                    var d when d < viewPort.RealPixelSize / 2 => Color.Red,
                    _ => Color.White,
                };

                image.SetPixel(col, row, color);
            }
        });
    }
}