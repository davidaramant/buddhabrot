using System.Runtime.CompilerServices;

namespace Buddhabrot.Core.Utilities;

// ReSharper disable once InconsistentNaming
public static class XYHash
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int GetHashCode16(int x, int y) => ((y & 0b11) << 2) | (x & 0b11);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int GetHashCode64(int x, int y) => ((y & 0b111) << 3) | (x & 0b111);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int GetHashCode256(int x, int y) => ((y & 0b1111) << 4) | (x & 0b1111);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int GetHashCode1024(int x, int y) => ((y & 0b11111) << 5) | (x & 0b11111);
}
