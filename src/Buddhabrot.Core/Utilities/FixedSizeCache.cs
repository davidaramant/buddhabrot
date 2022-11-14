namespace Buddhabrot.Core.Utilities;

public sealed class FixedSizeCache<TKey, TValue>
    where TKey : struct
    where TValue : struct
{
    private readonly RingBuffer<TKey> _keyCache;
    private readonly RingBuffer<TValue> _valueCache;

    public int Capacity => _keyCache.Capacity;
    public int Count => _keyCache.Count;

    public FixedSizeCache(int maxSize)
    {
        _keyCache = new(maxSize);
        _valueCache = new(maxSize);
    }

    public void Add(TKey key, TValue value)
    {
        foreach (var savedKey in _keyCache)
        {
            if (savedKey.Equals(key))
            {
                return;
            }
        }

        _keyCache.Add(key);
        _valueCache.Add(value);
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        int index = 0;
        foreach (var savedKey in _keyCache)
        {
            if (savedKey.Equals(key))
            {
                value = _valueCache[index];
                return true;
            }
            index++;
        }

        value = default;

        return false;
    }
}
