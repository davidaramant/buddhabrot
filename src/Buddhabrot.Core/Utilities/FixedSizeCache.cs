namespace Buddhabrot.Core.Utilities;

public sealed class FixedSizeCache<TKey, TValue> where TKey : struct where TValue : struct
{
    private readonly RingBuffer<(TKey Key, TValue Value)> _cache;

    public int Capacity => _cache.Capacity;
    public int Count => _cache.Count;

    public FixedSizeCache(int maxSize) => _cache = new(maxSize);

    public void Add(TKey key, TValue value)
    {
        foreach (var item in _cache)
        {
            if (Equals(item.Key, key))
            {
                return;
            }
        }

        _cache.Add((key, value));
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        foreach (var item in _cache)
        {
            if (item.Key.Equals(key))
            {
                value = item.Value;
                return true;
            }
        }

        value = default;

        return false;
    }
}
