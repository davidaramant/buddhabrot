using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Buddhabrot.Edges;
using NUnit.Framework;

namespace Tests.Edges
{
    [TestFixture]
    public sealed class EdgeAreaTests
    {
        [Test]
        public void ShouldReturnWhichCornersAreInAndNotInSet()
        {
            var edgeArea = new EdgeArea(new Point(), Corners.BottomLeft);

            Assert.That(edgeArea.GetCornersInSet().ToArray(), Is.EqualTo(new[] { Corners.BottomLeft }));
            Assert.That(edgeArea.GetCornersNotInSet().ToArray(), Is.EquivalentTo(new[] { Corners.TopLeft, Corners.TopRight, Corners.BottomRight, }));
        }
    }
}
