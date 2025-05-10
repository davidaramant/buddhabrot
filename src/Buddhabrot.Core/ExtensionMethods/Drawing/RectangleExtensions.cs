using SkiaSharp;

namespace Buddhabrot.Core.ExtensionMethods.Drawing;

public static class RectangleExtensions
{
	public static int GetArea(this SKSizeI size) => size.Width * size.Height;

	public static int GetArea(this SKRectI rect) => rect.Width * rect.Height;

	public static IEnumerable<SKPointI> GetAllPositions(this SKRectI rect)
	{
		for (int row = 0; row < rect.Height; row++)
		{
			for (int col = 0; col < rect.Width; col++)
			{
				yield return new SKPointI(col + rect.Left, row + rect.Top);
			}
		}
	}

	public static bool IsInvalid(this SKRectI rect) => rect.Width <= 0 || rect.Height <= 0;
}
