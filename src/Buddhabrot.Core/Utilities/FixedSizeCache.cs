namespace Buddhabrot.Core.Utilities;

public sealed class FixedSizeCache<TKey, TValue>
    where TKey : struct
    where TValue : struct
{
    private readonly TKey[] _keys;
    private readonly TValue[] _values;
    private readonly int _capacity;

    public FixedSizeCache(int capacity, TKey defaultKey)
    {
        _capacity = capacity;
        _keys = new TKey[capacity];
        Array.Fill(_keys, defaultKey);
        _values = new TValue[capacity];
    }

    public void Add(TKey key, TValue value)
    {
        var index = (uint)key.GetHashCode() % _capacity;
        _keys[index] = key;
        _values[index] = value;
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        var index = (uint)key.GetHashCode() % _capacity;
        if (_keys[index].Equals(key))
        {
            value = _values[index];
            return true;
        }

        value = default;

        return false;
    }
}