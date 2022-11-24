namespace Buddhabrot.Core.Boundary;

public readonly record struct QuadNode(uint Encoded)
{
    public static readonly QuadNode UnknownLeaf = MakeLeaf(RegionType.Unknown);
    public static readonly QuadNode UnknownLeafQuad = new((uint) NodeType.LeafQuad);
    
    public static QuadNode MakeLeaf(RegionType type) => new((uint) type << 2);

    public static QuadNode MakeLeaf(
        RegionType type,
        RegionType ll,
        RegionType lr,
        RegionType ul,
        RegionType ur) => new(
        (uint) ll << 10 |
        (uint) lr << 8 |
        (uint) ul << 6 |
        (uint) ur << 4 |
        (uint) type << 2 |
        (uint) NodeType.LeafQuad);

    public static QuadNode MakeBranch(RegionType type, int childIndex) =>
        new((uint) childIndex << 4 | (uint) type << 2 | (uint) NodeType.Branch);

    public NodeType NodeType => (NodeType) (Encoded & 0b11);
    public RegionType RegionType => (RegionType) (Encoded >> 2 & 0b11);

    public int ChildIndex => (int) (Encoded >> 4);
    public int GetChildIndex(Quadrant quadrant) => ChildIndex + (int) quadrant;

    public RegionType LL => (RegionType) (Encoded >> 10 & 0b11);
    public RegionType LR => (RegionType) (Encoded >> 8 & 0b11);
    public RegionType UL => (RegionType) (Encoded >> 6 & 0b11);
    public RegionType UR => (RegionType) (Encoded >> 4 & 0b11);

    public QuadNode WithLL(RegionType ll) => new(Encoded | ((uint) ll << 10));
    public QuadNode WithLR(RegionType lr) => new(Encoded | ((uint) lr << 8));
    public QuadNode WithUL(RegionType ul) => new(Encoded | ((uint) ul << 6));
    public QuadNode WithUR(RegionType ur) => new(Encoded | ((uint) ur << 4));

    public QuadNode WithQuadrant(Quadrant quadrant, RegionType type) =>
        quadrant switch
        {
            Quadrant.LL => WithLL(type),
            Quadrant.LR => WithLR(type),
            Quadrant.UL => WithUL(type),
            Quadrant.UR => WithUR(type),
            _ => throw new Exception("Can't happen")
        };
    
    public override string ToString() => $"{NodeType} {RegionType}" + NodeType switch
    {
        NodeType.Leaf => string.Empty,
        NodeType.Branch => $" {ChildIndex}",
        NodeType.LeafQuad => $" {LL} {LR} {UL} {UR}",
        _ => "how could this have happened"
    };
}