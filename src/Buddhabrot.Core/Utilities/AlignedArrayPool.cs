using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace Buddhabrot.Core.Utilities;

public sealed unsafe class AlignedArrayPool : IDisposable
{
	private readonly ConcurrentBag<ulong> _pool = new();

	public const int ArrayBytes = 4096;

	public void* Rent() => _pool.TryTake(out var array) ? (void*)array : NativeMemory.AlignedAlloc(ArrayBytes, 64);

	public void Return(void* array) => _pool.Add((ulong)array);

	public void Dispose()
	{
		foreach (var array in _pool)
		{
			NativeMemory.Free((void*)array);
		}
	}
}
