namespace Buddhabrot.Core.Boundary.QuadTrees;

public readonly struct QuadNode
{
    public readonly uint Encoded;

    public QuadNode(uint encoded) => Encoded = encoded;
    
    public static readonly QuadNode UnknownLeaf = MakeLeaf(RegionType.Empty);

    public static QuadNode MakeLeaf(RegionType type) => new((uint) type << 2);

    public static QuadNode MakeLeaf(
        RegionType type,
        RegionType sw,
        RegionType se,
        RegionType nw,
        RegionType ne) => new(
        (uint) sw << 10 |
        (uint) se << 8 |
        (uint) nw << 6 |
        (uint) ne << 4 |
        (uint) type << 2 |
        (uint) NodeType.LeafQuad);

    public static QuadNode MakeBranch(RegionType type, int childIndex) =>
        new((uint) childIndex << 4 | (uint) type << 2 | (uint) NodeType.Branch);
    
    public static QuadNode MakeLongBranch(int childIndex) =>
        new((uint) childIndex << 2 | (uint) NodeType.LongBranch);

    public NodeType NodeType => (NodeType) (Encoded & 0b11);
    public RegionType RegionType => (RegionType) (Encoded >> 2 & 0b11);

    public int LongChildIndex => (int) (Encoded >> 2);
    public int GetLongChildIndex(Quadrant quadrant) => LongChildIndex + (int) quadrant;
    
    public int ChildIndex => (int) (Encoded >> 4);
    public int GetChildIndex(Quadrant quadrant) => ChildIndex + (int) quadrant;

    public RegionType SW => (RegionType) (Encoded >> 10 & 0b11);
    public RegionType SE => (RegionType) (Encoded >> 8 & 0b11);
    public RegionType NW => (RegionType) (Encoded >> 6 & 0b11);
    public RegionType NE => (RegionType) (Encoded >> 4 & 0b11);

    public RegionType GetQuadrant(Quadrant quadrant)
    {
        var offset = QuadrantOffset(quadrant);
        return (RegionType) (Encoded >> offset & 0b11);
    }

    public QuadNode WithSW(RegionType sw) => new(Encoded | (((uint) sw << 10) + (int) NodeType.LeafQuad));
    public QuadNode WithSE(RegionType se) => new(Encoded | (((uint) se << 8) + (int) NodeType.LeafQuad));
    public QuadNode WithNW(RegionType nw) => new(Encoded | (((uint) nw << 6) + (int) NodeType.LeafQuad));
    public QuadNode WithNE(RegionType ne) => new(Encoded | (((uint) ne << 4) + (int) NodeType.LeafQuad));

    public QuadNode WithQuadrant(Quadrant quadrant, RegionType type)
    {
        var offset = QuadrantOffset(quadrant);
        return new(Encoded | ((uint) type << offset) + (int) NodeType.LeafQuad);
    }

    private static int QuadrantOffset(Quadrant quadrant) => 10 - 2 * (int) quadrant;

    public override string ToString() => NodeType + NodeType switch
    {
        NodeType.Leaf =>  $" {RegionType}",
        NodeType.Branch => $" {RegionType} {ChildIndex}",
        NodeType.LongBranch => $" {LongChildIndex}",
        NodeType.LeafQuad => $" ({RegionType}) SW:{SW} SE:{SE} NW:{NW} NE:{NE}",
        _ => "how could this have happened"
    };

    #region Equality
    
    public override int GetHashCode() => (int)Encoded;

    public override bool Equals(object? obj) => obj is QuadNode other && Equals(other);

    public bool Equals(QuadNode other) => Encoded == other.Encoded;

    public static bool operator ==(QuadNode left, QuadNode right) => left.Equals(right);

    public static bool operator !=(QuadNode left, QuadNode right) => !left.Equals(right);

    #endregion
}