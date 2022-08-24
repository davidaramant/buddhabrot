using System.Drawing;
using Buddhabrot.Core.Images;

namespace Buddhabrot.Core.Boundary;

public sealed class QuadTreeRenderer : IDisposable
{
    private readonly bool _leaveImageOpen;
    private const int CellWidth = 3;
    public int Levels { get; }
    public RasterImage Image { get; }

    public QuadTreeRenderer(int levels, int imageScale = 1, bool leaveImageOpen = false)
    {
        if (levels < 1)
            throw new ArgumentOutOfRangeException(nameof(levels));
        _leaveImageOpen = leaveImageOpen;
        Levels = levels;
        int width = 2 + (1 << (levels - 1)) * CellWidth + (1 << (levels - 1)) - 1;
        Image = new RasterImage(width, width, imageScale);
        Image.Fill(Color.Black);
    }

    public void DrawCell(int x, int y, int depth, Color c)
    {
        if (depth > (Levels - 1))
            throw new ArgumentOutOfRangeException(nameof(depth));

        var inverseDepth = Levels - depth - 1;
        var cellWidth = (1 << inverseDepth) * CellWidth + (1 << inverseDepth) - 1;

        var correctedY = (1 << depth) - y - 1;
        
        var cellX = 1 + (x * (cellWidth + 1));
        var cellY = 1 + (correctedY * (cellWidth + 1));

        Image.FillRectangle(cellX, cellY, cellWidth, cellWidth, c);
    }

    public void Dispose()
    {
        if (!_leaveImageOpen)
        {
            Image.Dispose();
        }
    }
}