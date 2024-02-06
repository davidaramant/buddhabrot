namespace Buddhabrot.Core.Boundary;

/// <summary>
/// A position offset with +Y going UP
/// </summary>
public readonly record struct Offset(int X, int Y)
{
	public static readonly Offset Right = new(1, 0);
	public static readonly Offset Left = new(-1, 0);
	public static readonly Offset Up = new(0, 1);
	public static readonly Offset Down = new(0, -1);

	public static readonly Offset UpRight = new(1, 1);
	public static readonly Offset UpLeft = new(-1, 1);
	public static readonly Offset DownRight = new(1, -1);
	public static readonly Offset DownLeft = new(-1, -1);
}
