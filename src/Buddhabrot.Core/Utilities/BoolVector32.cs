namespace Buddhabrot.Core.Utilities;

public readonly struct BoolVector32
{
	private readonly uint _data;

	public static readonly BoolVector32 Empty = new(0);

	public bool this[int index] => (_data & (1 << index)) != 0;

	private BoolVector32(uint data) => _data = data;

	public BoolVector32 WithBit(int index) => new(_data | (uint)(1 << index));
}
