namespace Buddhabrot.Core.Utilities;

public sealed class FixedSizeCache<TKey, TValue>
	where TKey : struct
	where TValue : struct
{
	private readonly TKey[] _keys;
	private readonly TValue[] _values;
	private readonly Func<TKey, int> _getIndex;

	public FixedSizeCache(int capacity, TKey defaultKey)
		: this(capacity, defaultKey, getIndex: key => (int)((uint)key.GetHashCode()) % capacity) { }

	public FixedSizeCache(int capacity, TKey defaultKey, Func<TKey, int> getIndex)
	{
		_getIndex = getIndex;
		_keys = new TKey[capacity];
		Array.Fill(_keys, defaultKey);
		_values = new TValue[capacity];
	}

	public void Add(TKey key, TValue value)
	{
		var index = _getIndex(key);
		_keys[index] = key;
		_values[index] = value;
	}

	public bool TryGetValue(TKey key, out TValue value)
	{
		var index = _getIndex(key);
		if (_keys[index].Equals(key))
		{
			value = _values[index];
			return true;
		}

		value = default;

		return false;
	}
}
