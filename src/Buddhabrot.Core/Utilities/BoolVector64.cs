namespace Buddhabrot.Core.Utilities;

public readonly struct BoolVector64
{
	private readonly ulong _data;

	public static readonly BoolVector64 Empty = new(0);

	public bool this[int index] => (_data & (ulong)(1 << index)) != 0;

	private BoolVector64(ulong data) => _data = data;

	public BoolVector64 WithBit(int index) => new(_data | ((ulong)1 << index));
}
