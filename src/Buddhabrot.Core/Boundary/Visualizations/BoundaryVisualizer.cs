using System.Drawing;
using Buddhabrot.Core.Images;
using Buddhabrot.Core.IterationKernels;

namespace Buddhabrot.Core.Boundary.Visualizations;

public static class BoundaryVisualizer
{
    public static RasterImage RenderBoundary(IReadOnlyList<RegionId> regions)
    {
        // TODO: Deal with flipped Y
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
                    ScalarKernel.FindEscapeTime(viewPort.GetComplex(x, y), iterationRange.Max) switch
                    {
                        {IsInfinite: true} => Color.LightSteelBlue,
                        var et when et.Iterations >= iterationRange.Min => Color.Red,
                        _ => Color.White,
                    });
            }
        });

        return img;
    }

    public static RasterImage RenderRegionLookup(RegionLookup lookup, int scale = 1)
    {
        var r = new QuadTreeRenderer(lookup.Levels, scale, leaveImageOpen: true);

        var nodes = lookup.GetRawNodes();

        (Quad SW, Quad NW, Quad NE, Quad SE) GetChildren(Quad quad)
        {
            var index = quad.ChildIndex;
            return (nodes[index], nodes[index + 1], nodes[index + 2], nodes[index + 3]);
        }

        void DrawQuad(Quad quad, int depth, int x, int y)
        {
            if (depth == lookup.Levels - 1 || !quad.HasChildren)
            {
                r.DrawCell(x, y, depth, PickColorFromType(quad.Type));
                return;
            }

            var multiplier = 1 << depth;
            var newX = x * multiplier;
            var newY = y * multiplier;
            var (sw, nw, ne, se) = GetChildren(quad);
            DrawQuad(sw, depth + 1, newX, newY);
            DrawQuad(nw, depth + 1, newX, newY + 1);
            DrawQuad(ne, depth + 1, newX + 1, newY + 1);
            DrawQuad(se, depth + 1, newX + 1, newY);
        }

        DrawQuad(nodes.Last(), depth: 0, 0, 0);

        return r.Image;
    }

    private static Color PickColorFromType(RegionType type) =>
        type switch
        {
            RegionType.Border => Color.DarkBlue,
            RegionType.Filament => Color.Red,
            _ => Color.White,
        };
}