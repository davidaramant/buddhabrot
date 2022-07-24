using System.Drawing;
using System.Linq;
using Buddhabrot.Core;
using Buddhabrot.IterationKernels;
using Buddhabrot.PointGrids;
using NUnit.Framework;

namespace Tests.PointGrids;

[TestFixture]
public sealed class PointGridTests
{
    [Test]
    public void ShouldRountTrip()
    {
        using (var tempFile = new TempFile())
        {
            var pointsInSet = new[]
            {
                true,
                true,
                true,
                true,

                false,
                true,
                true,
                false,

                false,
                false,
                true,
                false,

                false,
                false,
                false,
                false,
            };

            PointGrid.Write(
                tempFile.Path,
                new ViewPort(
                    new ComplexArea(new DoubleRange(-1, 1), new DoubleRange(-1, 1)),
                    new Size(4, 4)),
                ComputationType.ScalarFloat,
                pointsInSet);

            using (var pointGrid = PointGrid.Load(tempFile.Path))
            {
                var rows = pointGrid.ToArray();

                for (int rowIndex = 0; rowIndex < 4; rowIndex++)
                {
                    Assert.That(rows[rowIndex].Y, Is.EqualTo(rowIndex), "Wrong row Y position.");
                    Assert.That(
                        rows[rowIndex].ToArray(),
                        Is.EqualTo(pointsInSet.Skip(rowIndex * 4).Take(4).ToArray()),
                        "Did not return correct values for points in set.");
                }
            }
        }
    }
}