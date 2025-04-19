using BenchmarkDotNet.Attributes;
using Buddhabrot.Core.Boundary;
using Buddhabrot.Core.Utilities;
using CacheTable;

namespace Buddhabrot.Benchmarks;

/// <summary>
/// Different ways to do a fixed-size cache
/// </summary>
public class FixedSizeCacheBenchmarks
{
	public const int DataSize = 100;
	public const int LookupsPerData = 8;

	// Varying this doesn't tell us anything because we are not simulating the consequences of something not being in the cache.
	public const int CacheSize = 32;

	private RegionId[] _keys = [];
	private RegionId[] _lookups = [];

	[GlobalSetup]
	public void FillArrays()
	{
		var rand = new Random(0);
		_keys = new RegionId[DataSize];
		_lookups = new RegionId[DataSize * LookupsPerData];
		for (int i = 0; i < DataSize; i++)
		{
			_keys[i] = MakeId(i);

			for (int j = 0; j < LookupsPerData; j++)
			{
				var li = i * LookupsPerData + j;
				_lookups[li] = MakeId(i);
			}
		}

		RegionId MakeId(int i) => new(X: rand.Next(i, i + LookupsPerData), Y: rand.Next(i, i + LookupsPerData));
	}

	[Benchmark(Baseline = true)]
	public int LinearSearchRingBuffers()
	{
		var cache = new LinearSearchRingBuffersCache<RegionId, int>(CacheSize);

		int sum = 0;
		for (int i = 0; i < DataSize; i++)
		{
			cache.Add(_keys[i], _keys[i].X);

			for (int j = 0; j < LookupsPerData; j++)
			{
				var li = i * LookupsPerData + j;

				if (cache.TryGetValue(_lookups[li], out var val))
				{
					sum += val;
				}
			}
		}

		return sum;
	}

	[Benchmark]
	public int CacheTable()
	{
		var cache = new CacheTable<RegionId, int>(CacheSize, 4);

		int sum = 0;
		for (int i = 0; i < DataSize; i++)
		{
			cache[_keys[i]] = _keys[i].X;

			for (int j = 0; j < LookupsPerData; j++)
			{
				var li = i * LookupsPerData + j;

				if (cache.TryGetValue(_lookups[li], out var val))
				{
					sum += val;
				}
			}
		}

		return sum;
	}

	[Benchmark]
	public int HashingRingBuffer()
	{
		var cache = new HashingRingBufferCache<RegionId, int>(CacheSize, new RegionId(-1, -1));

		int sum = 0;
		for (int i = 0; i < DataSize; i++)
		{
			cache.Add(_keys[i], _keys[i].X);

			for (int j = 0; j < LookupsPerData; j++)
			{
				var li = i * LookupsPerData + j;

				if (cache.TryGetValue(_lookups[li], out var val))
				{
					sum += val;
				}
			}
		}

		return sum;
	}

	public sealed class LinearSearchRingBuffersCache<TKey, TValue>
		where TKey : struct
		where TValue : struct
	{
		private readonly RingBuffer<TKey> _keyCache;
		private readonly RingBuffer<TValue> _valueCache;

		public int Count => _keyCache.Count;

		public LinearSearchRingBuffersCache(int maxSize)
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

	public sealed class HashingRingBufferCache<TKey, TValue>
		where TKey : struct
		where TValue : struct
	{
		private readonly TKey[] _keys;
		private readonly TValue[] _values;
		private readonly int _capacity;

		public HashingRingBufferCache(int capacity, TKey defaultKey)
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
}
