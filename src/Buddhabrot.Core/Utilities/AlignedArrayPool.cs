using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace Buddhabrot.Core.Utilities;

public sealed unsafe class AlignedArrayPool : IDisposable
{
	private readonly ConcurrentBag<IntPtr> _pool = new();

	/// <summary>
	/// I think AVX2 would be fine with 32 (256 bits / 8) but this will also work for AVX512
	/// </summary>
	private const int Alignment = 64;
	public const int ArrayBytes = 4096;
	public int Count => _pool.Count;

	public IntPtr Rent() =>
		_pool.TryTake(out var array) ? array : new IntPtr(NativeMemory.AlignedAlloc(ArrayBytes, Alignment));

	public void Return(IntPtr array) => _pool.Add(array);

	public void Dispose()
	{
		foreach (var array in _pool)
		{
			NativeMemory.Free(array.ToPointer());
		}
	}
}
