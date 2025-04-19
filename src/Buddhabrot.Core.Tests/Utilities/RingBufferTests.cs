using Buddhabrot.Core.Utilities;

namespace Buddhabrot.Core.Tests.Utilities;

public class RingBufferTests
{
	[Fact]
	public void ShouldAddUpToCapacity()
	{
		const int capacity = 5;
		var ring = new RingBuffer<int>(capacity);

		for (int i = 0; i < 10; i++)
		{
			ring.Count.ShouldBe(Math.Min(i, capacity));
			ring.Add(i);
		}
	}

	[Fact]
	public void ShouldReplaceLastValue()
	{
		var ring = new RingBuffer<int>(3) { 1 };

		ring.ShouldBe(new[] { 1 });

		ring.Add(2);

		ring.ShouldBe(new[] { 1, 2 });

		ring.Add(3);

		ring.ShouldBe(new[] { 1, 2, 3 });

		ring.Add(4);

		ring.ShouldBe(new[] { 2, 3, 4 });

		ring.Add(5);

		ring.ShouldBe(new[] { 3, 4, 5 });
	}
}
