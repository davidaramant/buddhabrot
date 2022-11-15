using System.Drawing;

namespace Buddhabrot.Core.ExtensionMethods.Drawing;

public static class RectangleExtensions
{
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
}