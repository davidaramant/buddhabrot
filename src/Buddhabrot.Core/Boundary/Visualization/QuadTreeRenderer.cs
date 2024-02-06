using System.Drawing;
using Buddhabrot.Core.ExtensionMethods.Drawing;
using Buddhabrot.Core.Images;
using SkiaSharp;

namespace Buddhabrot.Core.Boundary.Visualization;

public sealed class QuadTreeRenderer
{
	private const int CellWidth = 3;
	private readonly int _levels;
	private readonly int _xOffset;
	private readonly RasterImage _image;

	public static int GetRequiredWidth(int levels) => 2 + (1 << (levels - 1)) * CellWidth + (1 << (levels - 1)) - 1;

	public QuadTreeRenderer(RasterImage image, int levels, int xOffset = 0)
	{
		_levels = levels;
		_xOffset = xOffset;
		_image = image;
	}

	public void DrawCell(int x, int y, int depth, Color c) => DrawCell(x, y, depth, c.ToSKColor());

	public void DrawCell(int x, int y, int depth, SKColor c)
	{
		var inverseDepth = _levels - depth - 1;
		var cellWidth = (1 << inverseDepth) * CellWidth + (1 << inverseDepth) - 1;

		var correctedY = (1 << depth) - y - 1;

		var cellX = 1 + (x * (cellWidth + 1));
		var cellY = 1 + (correctedY * (cellWidth + 1));

		_image.FillRectangle(cellX + _xOffset, cellY, cellWidth, cellWidth, c);
	}
}
