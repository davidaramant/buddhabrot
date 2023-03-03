using System.Drawing;

namespace Buddhabrot.Core.ExtensionMethods.Drawing;

public static class RectangleExtensions
{
    public static int GetArea(this Size size) => size.Width * size.Height;
    public static int GetArea(this Rectangle rect) => rect.Width * rect.Height;
    
    public static IEnumerable<Point> GetAllPositions(this Rectangle rect)
    {
        for (int row = 0; row < rect.Height; row++)
        {
            for (int col = 0; col < rect.Width; col++)
            {
                yield return new Point(col + rect.X, row + rect.Y);
            }
        }
    }

    public static bool IsInvalid(this Rectangle rect) => rect.Width <= 0 || rect.Height <= 0;
}