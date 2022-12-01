using BenchmarkDotNet.Attributes;
using Buddhabrot.Core.Utilities;
using CacheTable;

namespace Buddhabrot.Benchmarks;

public class FixedSizeCacheBenchmarks
{
    public const int DataSize = 1000;
    public const int CacheSize = 32;

    private int[] _data = Array.Empty<int>();

    [GlobalSetup]
    public void FillArrays()
    {
        var rand = new Random(0);
        _data = new int[DataSize];
        for (int i = 0; i < DataSize; i++)
        {
            _data[i] = rand.Next(CacheSize * 2);
        }
    }

    [Benchmark(Baseline = true)]
    public int DoubleRingBuffers()
    {
        var cache = new DoubleRingBuffersCache<int, int>(CacheSize);
        for (int i = 0; i < CacheSize; i++)
        {
            cache.Add(i,i);
        }

        int sum = 0;
        for (int i = 0; i < DataSize; i++)
        {
            if (cache.TryGetValue(i, out var val))
            {
                sum += val;
            }
        }

        return sum;
    }
    
    [Benchmark]
    public int SingleRingBuffer()
    {
        var cache = new SingleRingBufferCache<int, int>(CacheSize);
        for (int i = 0; i < CacheSize; i++)
        {
            cache.Add(i,i);
        }

        int sum = 0;
        for (int i = 0; i < DataSize; i++)
        {
            if (cache.TryGetValue(i, out var val))
            {
                sum += val;
            }
        }

        return sum;
    }
    
    [Benchmark]
    public int CacheTable()
    {
        var cache = new CacheTable<int, int>(CacheSize, 4);
        for (int i = 0; i < CacheSize; i++)
        {
            cache[i] = i;
        }

        int sum = 0;
        for (int i = 0; i < DataSize; i++)
        {
            if (cache.TryGetValue(i, out var val))
            {
                sum += val;
            }
        }

        return sum;
    }
    
    public sealed class DoubleRingBuffersCache<TKey, TValue>
        where TKey : struct
        where TValue : struct
    {
        private readonly RingBuffer<TKey> _keyCache;
        private readonly RingBuffer<TValue> _valueCache;

        public int Count => _keyCache.Count;

        public DoubleRingBuffersCache(int maxSize)
        {
            _keyCache = new(maxSize);
            _valueCache = new(maxSize);
        }

        public void Add(TKey key, TValue value)
        {
            _keyCache.Add(key);
            _valueCache.Add(value);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            // Use a for loop to avoid allocating an enumerator
            for (int i = 0; i < _keyCache.Count; i++)
            {
                if (_keyCache[i].Equals(key))
                {
                    value = _valueCache[i];
                    return true;
                }
            }

            value = default;

            return false;
        }
    }

    public sealed class SingleRingBufferCache<TKey, TValue>
        where TKey : struct
        where TValue : struct
    {
        private readonly RingBuffer<(TKey Key, TValue Value)> _cache;

        public int Count => _cache.Count;

        public SingleRingBufferCache(int maxSize)
        {
            _cache = new(maxSize);
        }

        public void Add(TKey key, TValue value)
        {
            _cache.Add((key, value));
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            // Use a for loop to avoid allocating an enumerator
            for (int i = 0; i < _cache.Count; i++)
            {
                if (_cache[i].Key.Equals(key))
                {
                    value = _cache[i].Value;
                    return true;
                }
            }

            value = default;

            return false;
        }
    }
}