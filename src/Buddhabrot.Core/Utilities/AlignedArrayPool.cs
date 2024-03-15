using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace Buddhabrot.Core.Utilities;

public sealed unsafe class AlignedArrayPool : IDisposable
{
	private readonly ConcurrentBag<IntPtr> _pool = new();

	public const int ArrayBytes = 4096;
	public int Count => _pool.Count;

	public IntPtr Rent() =>
		_pool.TryTake(out var array) ? array : new IntPtr(NativeMemory.AlignedAlloc(ArrayBytes, 64));

	public void Return(IntPtr array) => _pool.Add(array);

	public void Dispose()
	{
		foreach (var array in _pool)
		{
			NativeMemory.Free(array.ToPointer());
		}
	}
}
