using Buddhabrot.Core.Utilities;

namespace Buddhabrot.Core.Tests.Utilities;

public class FixedSizeCacheTests
{
	private readonly FixedSizeCache<int, int> _cache = new(4, defaultKey: -1);

	[Test]
	public void ShouldHandleEntriesNotInCache()
	{
		_cache.TryGetValue(0, out _).ShouldBeFalse();
	}

	[Test]
	public void ShouldLookUpAddedEntry()
	{
		_cache.Add(1, 2);

		_cache.TryGetValue(1, out var value).ShouldBeTrue();
		value.ShouldBe(2);
	}
}