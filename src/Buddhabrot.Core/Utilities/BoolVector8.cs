namespace Buddhabrot.Core.Utilities;

public readonly struct BoolVector8
{
	private readonly byte _data;

	public static readonly BoolVector8 Empty = new(0);

	public bool this[int index] => (_data & (1 << index)) != 0;

	private BoolVector8(byte data) => _data = data;

	public BoolVector8 WithBit(int index) => new((byte)(_data | (1 << index)));
}
