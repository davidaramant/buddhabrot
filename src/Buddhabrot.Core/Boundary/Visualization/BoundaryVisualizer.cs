using System.Diagnostics;
using System.Drawing;
using Buddhabrot.Core.Boundary.QuadTrees;
using Buddhabrot.Core.Images;
using Buddhabrot.Core.Calculations;
using SkiaSharp;

namespace Buddhabrot.Core.Boundary.Visualization;

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

    public static RasterImage RenderRegionLookup(
        RegionLookup lookup,
        int scale = 1,
        SKColor? backgroundColor = null,
        IBoundaryPalette? palette = null)
    {
        palette ??= PastelPalette.Instance;

        var widthOfTopLevelQuadrant = QuadTreeRenderer.GetRequiredWidth(lookup.Height - 1);
        var imageWidth = QuadTreeRenderer.GetRequiredWidth(lookup.Height);
        var image = new RasterImage(imageWidth, widthOfTopLevelQuadrant, scale);
        image.Fill(backgroundColor ?? SKColors.Black);

        var nodes = lookup.Nodes;

        (RegionNode SW, RegionNode LR, RegionNode UL, RegionNode UR) GetChildren(RegionNode quad)
        {
            Debug.Assert(quad.IsLeaf == false, "Attempted to get children of leaf node");
            return (
                nodes[quad.GetChildIndex(Quadrant.SW)],
                nodes[quad.GetChildIndex(Quadrant.SE)],
                nodes[quad.GetChildIndex(Quadrant.NW)],
                nodes[quad.GetChildIndex(Quadrant.NE)]);
        }

        void DrawQuad(QuadTreeRenderer r, RegionNode quad, int depth, int x, int y)
        {
            if (quad.IsLeaf)
            {
                r.DrawCell(x, y, depth, PickColorFromType(quad.RegionType));
                return;
            }

            var newX = x << 1;
            var newY = y << 1;

            var (sw, se, nw, ne) = GetChildren(quad);
            DrawQuad(r, sw, depth + 1, newX, newY);
            DrawQuad(r, se, depth + 1, newX + 1, newY);
            DrawQuad(r, nw, depth + 1, newX, newY + 1);
            DrawQuad(r, ne, depth + 1, newX + 1, newY + 1);
        }

        var (_, _, topW, topE) = GetChildren(nodes.Last());
        DrawQuad(new QuadTreeRenderer(image, lookup.Height - 1), topW, depth: 0, 0, 0);
        DrawQuad(new QuadTreeRenderer(image, lookup.Height - 1, xOffset: widthOfTopLevelQuadrant - 1), topE, depth: 0,
            0, 0);

        return image;

        SKColor PickColorFromType(LookupRegionType type) =>
            type switch
            {
                LookupRegionType.Border => palette.Border,
                LookupRegionType.Filament => palette.Filament,
                LookupRegionType.Empty => palette.InBounds,
                _ => throw new Exception("Unknown region type")
            };
    }
}