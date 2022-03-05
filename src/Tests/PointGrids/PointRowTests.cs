using System.Linq;
using Buddhabrot.PointGrids;
using NUnit.Framework;

namespace Tests.PointGrids;

[TestFixture]
public sealed class PointRowTests
{
    [Test]
    public void ShouldReturnPointsInSet()
    {
        //0123456789
        //  SSS SSSS

        var row = new PointRow(width: 10, y: 0, segments: new[]
        {
            (false,2),
            (true,3),
            (false,1),
            (true,4),
        });

        var edgeLocations = row.GetXPositionsOfSet().ToArray();

        Assert.That(
            edgeLocations,
            Is.EqualTo(new[] { 2, 3, 4, 6, 7, 8, 9 }),
            "Did not return set positions.");
    }

    [Test]
    public void ShouldReturnCorrectValuesFromIndexer()
    {
        //0123456789
        //  SSS SSSS

        var row = new PointRow(width: 10, y: 0, segments: new[]
        {
            (false,2),
            (true,3),
            (false,1),
            (true,4),
        });

        var expected = row.ToArray();

        for (int i = 0; i < row.Width; i++)
        {
            Assert.That(row[i], Is.EqualTo(expected[i]), $"Unexpected value at index {i}");
        }
    }
}