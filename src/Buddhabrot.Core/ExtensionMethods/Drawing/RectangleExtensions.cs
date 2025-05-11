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

	public static SKRectI OffsetBy(this SKRectI rect, int x, int y) =>
		SKRectI.Create(rect.Left + x, rect.Top + y, rect.Width, rect.Height);

	public static SKSizeI Quarter(this SKSizeI size) => new(size.Width / 2, size.Height / 2);
}
