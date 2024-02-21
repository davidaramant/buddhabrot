using Buddhabrot.Core.Utilities;

namespace Buddhabrot.Core.Tests.Utilities;

public sealed class WorkRemainingTests
{
    [Fact]
    public void ShouldReturnFromSequenceIfNoAdditionalItems()
    {
        const int size = 5;
        using (var work = new WorkRemaining<int>(Enumerable.Repeat(size, size)))
        {
            work.Take(size).Should().HaveCount(size);
        }
    }

    [Fact]
    public void ShouldReturnAsMuchDataAsAvailable()
    {
        const int size = 5;
        using (var work = new WorkRemaining<int>(Enumerable.Repeat(size, size)))
        {
            work.Take(10 * size).Should().HaveCount(size);
        }
    }

    [Fact]
    public void ShouldReturnFromBufferBeforeSequence()
    {
        using (var work = new WorkRemaining<int>(Enumerable.Repeat(5, 5)))
        {
            work.AddAdditional(Enumerable.Repeat(3, 3));

            var dataReturned = work.Take(8).ToArray();
            var expected = Enumerable.Repeat(3, 3).Concat(Enumerable.Repeat(5, 5)).ToArray();

            dataReturned.Should().BeEquivalentTo(expected);
        }
    }

    [Fact]
    public void ShouldTakeLessThanTotal()
    {
        using (var work = new WorkRemaining<int>(Enumerable.Repeat(5, 5)))
        {
            work.AddAdditional(Enumerable.Repeat(3, 3));

            var dataReturned = work.Take(4).ToArray();
            var expected = Enumerable.Repeat(3, 3).Concat(Enumerable.Repeat(5, 1)).ToArray();

            dataReturned.Should().BeEquivalentTo(expected);
        }
    }

    [Fact]
    public void ShouldNotReturnDuplicatedWork()
    {
        using (var work = new WorkRemaining<int>(Enumerable.Repeat(5, 5)))
        {
            work.Take(4).Should().HaveCount(4);
            work.Take(4).Should().HaveCount(1);
        }
    }
}
