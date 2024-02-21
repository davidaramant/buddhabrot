using System.Buffers;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using Buddhabrot.Core.Boundary.QuadTrees;
using Buddhabrot.Core.Calculations;
using Buddhabrot.Core.ExtensionMethods.Drawing;
using Buddhabrot.Core.Images;
using SkiaSharp;

namespace Buddhabrot.Core.Boundary.Visualization;

public static class BoundaryVisualizer
{
    public static RasterImage RenderBorderRegion(
        Size resolution,
        AreaDivisions divisions,
        RegionId regionId,
        IterationRange iterationRange
    )
    {
        var viewPort = ViewPort.FromRegionId(resolution, divisions, regionId);
        return RenderBorderRegion(viewPort, iterationRange);
    }

    public static RasterImage RenderBorderRegion(ViewPort viewPort, IterationRange iterationRange)
    {
        var img = new RasterImage(viewPort.Resolution.Width, viewPort.Resolution.Height);

        var numPoints = viewPort.Resolution.GetArea();
        var points = ArrayPool<Complex>.Shared.Rent(numPoints);
        var results = ArrayPool<EscapeTime>.Shared.Rent(numPoints);

        for (int y = 0; y < img.Height; y++)
        {
            for (int x = 0; x < img.Width; x++)
            {
                points[y * img.Width + x] = viewPort.GetComplex(x, y);
            }
        }

        VectorKernel.FindEscapeTimes(points, results, numPoints: numPoints, maxIterations: iterationRange.Max);

        for (int y = 0; y < img.Height; y++)
        {
            for (int x = 0; x < img.Width; x++)
            {
                img.SetPixel(
                    x,
                    y,
                    results[y * img.Width + x] switch
                    {
                        { IsInfinite: true } => Color.MidnightBlue,
                        var et when et.Iterations >= iterationRange.Min => Color.Red,
                        _ => Color.White,
                    }
                );
            }
        }

        ArrayPool<Complex>.Shared.Return(points);
        ArrayPool<EscapeTime>.Shared.Return(results);

        return img;
    }

    public static RasterImage RenderRegionLookup(
        RegionLookup lookup,
        int scale = 1,
        SKColor? backgroundColor = null,
        IBoundaryPalette? palette = null
    )
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
                nodes[quad.GetChildIndex(Quadrant.NE)]
            );
        }

        void DrawQuad(QuadTreeRenderer r, RegionNode quad, int depth, int x, int y)
        {
            if (quad.IsLeaf)
            {
                r.DrawCell(x, y, depth, palette[quad.RegionType]);
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
        DrawQuad(
            new QuadTreeRenderer(image, lookup.Height - 1, xOffset: widthOfTopLevelQuadrant - 1),
            topE,
            depth: 0,
            0,
            0
        );

        return image;
    }
}
