namespace Buddhabrot.Core.Boundary.QuadTrees;

public enum VisitNodeType
{
	Leaf,
	Branch,
	LeafQuad,
}

public readonly struct VisitNode(uint encoded)
{
	public readonly uint Encoded = encoded;

	public static readonly VisitNode Unknown = MakeLeaf(VisitedRegionType.Unknown);

	public static VisitNode MakeLeaf(VisitedRegionType type) => new((uint)type << 2);

	public static VisitNode MakeLeaf(
		VisitedRegionType sw,
		VisitedRegionType se,
		VisitedRegionType nw,
		VisitedRegionType ne
	) => new((uint)sw << 8 | (uint)se << 6 | (uint)nw << 4 | (uint)ne << 2 | 0b10);

	public static VisitNode MakeBranch(int childIndex) => new((uint)childIndex << 1 | 1);

	public bool IsBranch => (Encoded & 1) == 1;
	public bool IsLeaf => (Encoded & 0b11) == 0b00;
	public bool IsLeafQuad => (Encoded & 0b11) == 0b10;

	public VisitNodeType NodeType
	{
		get
		{
			if ((Encoded & 1) == 1)
				return VisitNodeType.Branch;

			return (VisitNodeType)(Encoded & 0b11);
		}
	}

	public VisitedRegionType RegionType => (VisitedRegionType)(Encoded >> 2 & 0b11);

	public int ChildIndex => (int)(Encoded >> 1);

	public int GetChildIndex(Quadrant quadrant) => ChildIndex + (int)quadrant;

	public VisitedRegionType SW => (VisitedRegionType)(Encoded >> 8 & 0b11);
	public VisitedRegionType SE => (VisitedRegionType)(Encoded >> 6 & 0b11);
	public VisitedRegionType NW => (VisitedRegionType)(Encoded >> 4 & 0b11);
	public VisitedRegionType NE => (VisitedRegionType)(Encoded >> 2 & 0b11);

	public VisitedRegionType GetQuadrant(Quadrant quadrant)
	{
		var offset = QuadrantOffset(quadrant);
		return (VisitedRegionType)(Encoded >> offset & 0b11);
	}

	public VisitNode WithSW(VisitedRegionType sw) => new(Encoded | ((uint)sw << 8) | 0b10);

	public VisitNode WithSE(VisitedRegionType se) => new(Encoded | ((uint)se << 6) | 0b10);

	public VisitNode WithNW(VisitedRegionType nw) => new(Encoded | ((uint)nw << 4) | 0b10);

	public VisitNode WithNE(VisitedRegionType ne) => new(Encoded | ((uint)ne << 2) | 0b10);

	public VisitNode WithQuadrant(Quadrant quadrant, VisitedRegionType type)
	{
		var offset = QuadrantOffset(quadrant);
		return new(Encoded | ((uint)type << offset) | 0b10);
	}

	private static int QuadrantOffset(Quadrant quadrant) => 8 - 2 * (int)quadrant;

	public override string ToString() =>
		NodeType
		+ NodeType switch
		{
			VisitNodeType.Leaf => $" {RegionType}",
			VisitNodeType.Branch => $" {ChildIndex}",
			VisitNodeType.LeafQuad => $" SW:{SW} SE:{SE} NW:{NW} NE:{NE}",
			_ => "how could this have happened"
		};

	#region Equality

	public override int GetHashCode() => (int)Encoded;

	public override bool Equals(object? obj) => obj is VisitNode other && Equals(other);

	public bool Equals(VisitNode other) => Encoded == other.Encoded;

	public static bool operator ==(VisitNode left, VisitNode right) => left.Equals(right);

	public static bool operator !=(VisitNode left, VisitNode right) => !left.Equals(right);

	#endregion
}
