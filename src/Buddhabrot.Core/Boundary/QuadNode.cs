namespace Buddhabrot.Core.Boundary;

public readonly record struct QuadNode(uint Encoded)
{
    public static QuadNode MakeLeaf(RegionType type) => new((uint)type << 2);

    public static QuadNode MakeLeaf(
        RegionType type, 
        RegionType ll, 
        RegionType lr, 
        RegionType ul, 
        RegionType ur) => new(
            (uint)ll << 10 | 
            (uint)lr << 8 | 
            (uint)ul << 6 | 
            (uint)ur << 4 | 
            (uint)type << 2 | 
            (uint)NodeType.LeafQuad);

    public static QuadNode MakeBranch(RegionType type, int childIndex) =>
        new((uint)childIndex << 4 | (uint) type << 2 | (uint) NodeType.Branch);
    
    public NodeType NodeType => (NodeType) (Encoded & 0b11);
    public RegionType RegionType => (RegionType) (Encoded >> 2 & 0b11);

    public int ChildIndex => (int) (Encoded >> 4);

    public RegionType LL => (RegionType) (Encoded >> 10 & 0b11);
    public RegionType LR => (RegionType) (Encoded >> 8 & 0b11);
    public RegionType UL => (RegionType) (Encoded >> 6 & 0b11);
    public RegionType UR => (RegionType) (Encoded >> 4 & 0b11);

    public override string ToString() => $"{NodeType} {RegionType}" + NodeType switch
    {
        NodeType.Leaf => string.Empty,
        NodeType.Branch => $" {ChildIndex}",
        NodeType.LeafQuad => $" {LL} {LR} {UL} {UR}",
        _ => "how could this have happened"
    };
}