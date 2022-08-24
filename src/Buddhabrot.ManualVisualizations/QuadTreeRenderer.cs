using System.Drawing;
using Buddhabrot.Core.Images;

namespace Buddhabrot.ManualVisualizations;

public sealed class QuadTreeRenderer : IDisposable
{
    private const int CellWidth = 3;
    public int MaxDepth { get; }
    public RasterImage Image { get; }

    public QuadTreeRenderer(int maxDepth)
    {
        if (maxDepth < 1)
            throw new ArgumentOutOfRangeException(nameof(maxDepth));
        MaxDepth = maxDepth;
        int width = 2 + (1 << (maxDepth - 1)) * CellWidth + (1 << (maxDepth - 1)) - 1;
        Image = new RasterImage(width, width);
        Image.Fill(Color.Black);
    }

    public void DrawCell(int x, int y, int depth, Color c)
    {
        if (depth > (MaxDepth - 1))
            throw new ArgumentOutOfRangeException(nameof(depth));

        var inverse = MaxDepth - depth - 1;
        var cellWidth = (1 << inverse) * CellWidth + (1 << inverse) - 1;

        var cellX = 1 + (x * (cellWidth + 1));
        var cellY = 1 + (y * (cellWidth + 1));

        Image.FillRectangle(cellX, cellY, cellWidth, cellWidth, c);
    }

    public void Dispose() => Image.Dispose();
}