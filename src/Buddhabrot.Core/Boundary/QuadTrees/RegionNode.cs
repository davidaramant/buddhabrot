namespace Buddhabrot.Core.Boundary.QuadTrees;

public readonly record struct RegionNode(uint Encoded)
{
	public static readonly RegionNode Empty = MakeLeaf(LookupRegionType.Empty);

	public static RegionNode MakeLeaf(LookupRegionType type) => new((uint)type << 1);

	public static RegionNode MakeBranch(LookupRegionType type, int childIndex) =>
		new((uint)childIndex << 4 | (uint)type << 1 | 1);

	public bool IsLeaf => (Encoded & 1) == 0;

	public LookupRegionType RegionType => (LookupRegionType)(Encoded >> 1 & 0b111);

	public int ChildIndex => (int)(Encoded >> 4);

	public int GetChildIndex(Quadrant quadrant) => ChildIndex + (int)quadrant;

	public override string ToString() => IsLeaf ? $"Leaf {RegionType}" : $"Branch {RegionType} {ChildIndex}";
}
