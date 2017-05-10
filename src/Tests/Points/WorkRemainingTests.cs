using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Buddhabrot.Core;
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
            using (var work = new WorkRemaining(GetPointSequence(size)))
            {
                Assert.That(work.Take(size).Count(), Is.EqualTo(size));
            }
        }

        [Test]
        public void ShouldReturnAsMuchDataAsAvailable()
        {
            const int size = 5;
            using (var work = new WorkRemaining(GetPointSequence(size)))
            {
                Assert.That(work.Take(10 * size).Count(), Is.EqualTo(size));
            }
        }

        [Test]
        public void ShouldReturnFromBufferBeforeSequence()
        {
            using (var work = new WorkRemaining(GetPointSequence(5)))
            {
                work.AddAdditional(GetPointSequence(3));

                var dataReturned = work.Take(8).Select(pair => (int) pair.InSet.Real).ToArray();
                var expected = Enumerable.Repeat(3, 3).Concat(Enumerable.Repeat(5, 5)).ToArray();

                Assert.That(dataReturned, Is.EqualTo(expected));
            }
        }

        [Test]
        public void ShouldTakeLessThanTotal()
        {
            using (var work = new WorkRemaining(GetPointSequence(5)))
            {
                work.AddAdditional(GetPointSequence(3));

                var dataReturned = work.Take(4).Select(pair => (int) pair.InSet.Real).ToArray();
                var expected = Enumerable.Repeat(3, 3).Concat(Enumerable.Repeat(5, 1)).ToArray();

                Assert.That(dataReturned, Is.EqualTo(expected));
            }
        }

        [Test]
        public void ShouldNotReturnDuplicatedWork()
        {
            using (var work = new WorkRemaining(GetPointSequence(5)))
            {
                Assert.That(work.Take(4).Count(), Is.EqualTo(4));
                Assert.That(work.Take(4).Count(), Is.EqualTo(1));
            }
        }

        private static IEnumerable<EdgeSpan> GetPointSequence(int size) =>
            Enumerable.Repeat(size, size).
            Select(i => new EdgeSpan(new Complex(i, i), new Complex(i, i)));
    }
}
