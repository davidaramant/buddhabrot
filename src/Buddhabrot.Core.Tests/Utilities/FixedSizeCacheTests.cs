using Buddhabrot.Core.Utilities;

namespace Buddhabrot.Core.Tests.Utilities;

public class FixedSizeCacheTests
{
    private readonly FixedSizeCache<int, int> _cache = new(4);

    [Fact]
    public void ShouldHandleEntriesNotInCache()
    {
        _cache.TryGetValue(0, out _).Should().BeFalse();
    }

    [Fact]
    public void ShouldLookUpAddedEntry()
    {
        _cache.Add(1, 2);

        _cache.TryGetValue(1, out var value).Should().BeTrue();
        value.Should().Be(2);
    }

    [Fact]
    public void ShouldRollOverContents()
    {
        for (int i = 0; i < _cache.Count + 1; i++)
        {
            _cache.Add(i, i);
        }

        _cache.TryGetValue(0, out _).Should().BeFalse();
    }
}
