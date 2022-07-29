using System.Drawing;
using Buddhabrot.Core.Images;

namespace Buddhabrot.Core.Boundary;

public static class BoundaryVisualizer
{
    public static RasterImage RenderBorderRegions(IReadOnlyList<RegionId> regions)
    {
        var maxBounds = regions.Aggregate<RegionId, (int X, int Y)>((0, 0),
            (max, areaId) => (Math.Max(max.X, areaId.X), Math.Max(max.Y, areaId.Y)));

        using var img = new RasterImage(width: maxBounds.X + 1, height: maxBounds.Y + 1);
        img.Fill(Color.White);
        foreach (var region in regions)
        {
            img.SetPixel(region.X, region.Y, Color.Red);
        }

        return img;
    }
}