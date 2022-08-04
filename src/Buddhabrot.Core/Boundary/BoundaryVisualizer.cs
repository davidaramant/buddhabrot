using System.Drawing;
using Buddhabrot.Core.Images;
using Buddhabrot.Core.IterationKernels;

namespace Buddhabrot.Core.Boundary;

public static class BoundaryVisualizer
{
    public static RasterImage RenderBoundary(IReadOnlyList<RegionId> regions)
    {
        var maxBounds = regions.Aggregate<RegionId, (int X, int Y)>((0, 0),
            (max, areaId) => (Math.Max(max.X, areaId.X), Math.Max(max.Y, areaId.Y)));

        var img = new RasterImage(width: maxBounds.X + 1, height: maxBounds.Y + 1);
        img.Fill(Color.White);
        foreach (var region in regions)
        {
            img.SetPixel(region.X, region.Y, Color.Red);
        }

        return img;
    }

    public static RasterImage RenderBorderRegion(ViewPort viewPort, IterationRange iterationRange)
    {
        var img = new RasterImage(viewPort.Resolution.Width, viewPort.Resolution.Height);

        Parallel.For(0, img.Height, y =>
        {
            for (int x = 0; x < img.Width; x++)
            {
                img.SetPixel(x, y,
                    ScalarDoubleKernel.FindEscapeTime(viewPort.GetComplex(x, y), iterationRange.Max) switch
                    {
                        {IsInfinite: true} => Color.LightSteelBlue,
                        var et when et.Iterations >= iterationRange.Min => Color.Red,
                        _ => Color.White,
                    });
            }
        });

        return img;
    }
}