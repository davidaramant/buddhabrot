using System.Runtime.CompilerServices;
using Buddhabrot.Core.Utilities;

namespace Buddhabrot.Core.Boundary.Classifiers;

public readonly record struct RegionBatchId(int X, int Y)
{
	public const int CornerPower = 2;
	public const int CornerWidth = 1 << CornerPower;
	public const int CornerArea = CornerWidth * CornerWidth;
	public const int RegionPower = 1;
	public const int RegionWidth = 1 << RegionPower;
	public const int RegionArea = RegionWidth * RegionWidth;

	public static readonly RegionBatchId Invalid = new(int.MaxValue, int.MaxValue);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public CornerId GetBottomLeftCorner() => new(X * CornerWidth, Y * CornerWidth);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public RegionId GetBottomLeftRegion() => new(X * RegionWidth, Y * RegionWidth);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetHashCode16() => XYHash.GetHashCode16(X, Y);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetHashCode64() => XYHash.GetHashCode64(X, Y);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetHashCode256() => XYHash.GetHashCode256(X, Y);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetHashCode1024() => XYHash.GetHashCode1024(X, Y);
}
