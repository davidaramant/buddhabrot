namespace Buddhabrot.Core.Utilities;

public readonly struct BoolVector16
{
	private readonly ushort _data;

	public static readonly BoolVector16 Empty = new(0);

	public int this[int index] => (_data >> index) & 1;

	private BoolVector16(ushort data) => _data = data;

	public BoolVector16 WithBit(int index) => new((ushort)(_data | (1 << index)));
}
