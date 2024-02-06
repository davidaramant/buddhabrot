namespace Buddhabrot.Core.Utilities;

public sealed class RingBuffer<T> : IReadOnlyCollection<T>
{
	private readonly T[] _buffer;
	private int _head = 0;
	private int _tail = 0;
	public int Capacity => _buffer.Length;
	public int Count { get; private set; }
	public T this[int index] => _buffer[(index + _head) % Capacity];

	public RingBuffer(int capacity)
	{
		_buffer = new T[capacity];
	}

	public void Add(T item)
	{
		_buffer[_tail] = item;

		_tail = (_tail + 1) % Capacity;

		if (Count == Capacity)
		{
			_head = (_head + 1) % Capacity;
		}

		Count = Math.Min(Count + 1, Capacity);
	}

	public void AddRange(IEnumerable<T> items)
	{
		foreach (var item in items)
		{
			Add(item);
		}
	}

	public IEnumerator<T> GetEnumerator()
	{
		for (int i = 0; i < Count; i++)
		{
			var index = (i + _head) % Capacity;
			yield return _buffer[index];
		}
	}

	System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}
