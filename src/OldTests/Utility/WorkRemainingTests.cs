using System.Linq;
using Buddhabrot.Utility;
using NUnit.Framework;

namespace Tests.Utility;

[TestFixture]
public sealed class WorkRemainingTests
{
    [Test]
    public void ShouldReturnFromSequenceIfNoAdditionalItems()
    {
        const int size = 5;
        using (var work = new WorkRemaining<int>(Enumerable.Repeat(size,size)))
        {
            Assert.That(work.Take(size).Count(), Is.EqualTo(size));
        }
    }

    [Test]
    public void ShouldReturnAsMuchDataAsAvailable()
    {
        const int size = 5;
        using (var work = new WorkRemaining<int>(Enumerable.Repeat(size, size)))
        {
            Assert.That(work.Take(10 * size).Count(), Is.EqualTo(size));
        }
    }

    [Test]
    public void ShouldReturnFromBufferBeforeSequence()
    {
        using (var work = new WorkRemaining<int>(Enumerable.Repeat(5, 5)))
        {
            work.AddAdditional(Enumerable.Repeat(3, 3));

            var dataReturned = work.Take(8).ToArray();
            var expected = Enumerable.Repeat(3, 3).Concat(Enumerable.Repeat(5, 5)).ToArray();

            Assert.That(dataReturned, Is.EqualTo(expected));
        }
    }

    [Test]
    public void ShouldTakeLessThanTotal()
    {
        using (var work = new WorkRemaining<int>(Enumerable.Repeat(5, 5)))
        {
            work.AddAdditional(Enumerable.Repeat(3,3));

            var dataReturned = work.Take(4).ToArray();
            var expected = Enumerable.Repeat(3, 3).Concat(Enumerable.Repeat(5, 1)).ToArray();

            Assert.That(dataReturned, Is.EqualTo(expected));
        }
    }

    [Test]
    public void ShouldNotReturnDuplicatedWork()
    {
        using (var work = new WorkRemaining<int>(Enumerable.Repeat(5, 5)))
        {
            Assert.That(work.Take(4).Count(), Is.EqualTo(4));
            Assert.That(work.Take(4).Count(), Is.EqualTo(1));
        }
    }
}