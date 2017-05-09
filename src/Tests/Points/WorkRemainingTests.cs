using System.Collections.Generic;
using System.Linq;
using Buddhabrot.Core;
using Buddhabrot.Edges;
using Buddhabrot.Points;
using NUnit.Framework;

namespace Tests.Points
{
    [TestFixture]
    public sealed class WorkRemainingTests
    {
        [Test]
        public void ShouldReturnFromSequenceIfNoAdditionalItems()
        {
            const int size = 5;
            var work = new WorkRemaining(GetPointSequence(size));

            Assert.That(work.Take(size).Count(), Is.EqualTo(size));
        }

        [Test]
        public void ShouldReturnAsMuchDataAsAvailable()
        {
            const int size = 5;
            var work = new WorkRemaining(GetPointSequence(size));

            Assert.That(work.Take(10 * size).Count(), Is.EqualTo(size));
        }

        [Test]
        public void ShouldReturnFromBufferBeforeSequence()
        {
            var work = new WorkRemaining(GetPointSequence(5));
            work.AddAdditional(GetPointSequence(3));

            var dataReturned = work.Take(8).Select(pair => (int) pair.InSet.Real).ToArray();
            var expected = Enumerable.Repeat(3, 3).Concat(Enumerable.Repeat(5, 5)).ToArray();

            Assert.That(dataReturned,Is.EqualTo(expected));
        }

        [Test]
        public void ShouldTakeLessThanTotal()
        {
            var work = new WorkRemaining(GetPointSequence(5));
            work.AddAdditional(GetPointSequence(3));

            var dataReturned = work.Take(4).Select(pair => (int)pair.InSet.Real).ToArray();
            var expected = Enumerable.Repeat(3, 3).Concat(Enumerable.Repeat(5, 1)).ToArray();

            Assert.That(dataReturned, Is.EqualTo(expected));
        }

        private static IEnumerable<PointPair> GetPointSequence(int size) =>
            Enumerable.Repeat(size, size).
            Select(i => new PointPair(new FComplex(i, i), new FComplex(i, i)));
    }
}
