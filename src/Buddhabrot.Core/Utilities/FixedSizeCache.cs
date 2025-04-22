namespace Buddhabrot.Core.Utilities;

public sealed class FixedSizeCache<TKey, TValue>
	where TKey : struct, IEquatable<TKey>
	where TValue : struct
{
	private readonly TKey[] _keys;
	private readonly TValue[] _values;
	private readonly Func<TKey, int> _getIndex;

	public FixedSizeCache(int capacity, TKey defaultKey)
		: this(capacity, defaultKey, getIndex: key => unchecked((int)((uint)key.GetHashCode() % (uint)capacity))) { }

	public FixedSizeCache(int capacity, TKey defaultKey, Func<TKey, int> getIndex)
	{
		_getIndex = getIndex;
		_keys = new TKey[capacity];
		_values = new TValue[capacity];
		_keys.AsSpan().Fill(defaultKey);
	}

	public bool Add(TKey key, TValue value)
	{
		var index = _getIndex(key);
		var oldKey = _keys[index];
		_keys[index] = key;
		_values[index] = value;

		return !oldKey.Equals(key);
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
