using System.Diagnostics;
using System.Drawing;
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

        (QuadNode LL, QuadNode LR, QuadNode UL, QuadNode UR) GetChildren(QuadNode quad)
        {
            Debug.Assert(quad.NodeType == NodeType.Branch, "Attempted to get children of leaf node");
            return (
                nodes[quad.GetChildIndex(Quadrant.LL)],
                nodes[quad.GetChildIndex(Quadrant.LR)],
                nodes[quad.GetChildIndex(Quadrant.UL)],
                nodes[quad.GetChildIndex(Quadrant.UR)]);
        }

        void DrawQuad(QuadTreeRenderer r, QuadNode quad, int depth, int x, int y)
        {
            if (quad.NodeType == NodeType.Leaf)
            {
                r.DrawCell(x, y, depth, PickColorFromType(quad.RegionType));
                return;
            }

            var newX = x << 1;
            var newY = y << 1;

            if (quad.NodeType == NodeType.LeafQuad)
            {
                r.DrawCell(newX, newY, depth + 1, PickColorFromType(quad.LL));
                r.DrawCell(newX + 1, newY, depth + 1, PickColorFromType(quad.LR));
                r.DrawCell(newX, newY + 1, depth + 1, PickColorFromType(quad.UL));
                r.DrawCell(newX + 1, newY + 1, depth + 1, PickColorFromType(quad.UR));
                return;
            }

            var (ll, lr, ul, ur) = GetChildren(quad);
            DrawQuad(r, ll, depth + 1, newX, newY);
            DrawQuad(r, lr, depth + 1, newX + 1, newY);
            DrawQuad(r, ul, depth + 1, newX, newY + 1);
            DrawQuad(r, ur, depth + 1, newX + 1, newY + 1);
        }

        var (_, _, topW, topE) = GetChildren(nodes.Last());
        DrawQuad(new QuadTreeRenderer(image, lookup.Height - 1), topW, depth: 0, 0, 0);
        DrawQuad(new QuadTreeRenderer(image, lookup.Height - 1, xOffset: widthOfTopLevelQuadrant - 1), topE, depth: 0,
            0, 0);

        return image;

        SKColor PickColorFromType(RegionType type) =>
            type switch
            {
                RegionType.Border => palette.Border,
                RegionType.Filament => palette.Filament,
                RegionType.Rejected => palette.InSet,
                RegionType.Unknown => palette.InBounds,
                _ => throw new Exception("Unknown region type")
            };
    }
}