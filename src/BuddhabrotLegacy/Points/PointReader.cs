using System.Numerics;
using Buddhabrot.Extensions;

namespace Buddhabrot.Points;

static class PointReader
{
    public static IEnumerable<Complex> ReadPoints(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        using var reader = new BinaryReader(stream);
        while (stream.Position != stream.Length)
        {
            yield return reader.ReadComplex();
        }
    }
}