namespace Buddhabrot.Core.Boundary.QuadTrees;

public enum VisitNodeType
{
    Leaf,
    Branch,
    LeafQuad,
}

public readonly struct VisitNode
{
    public readonly uint Encoded;

    public VisitNode(uint encoded) => Encoded = encoded;

    public static readonly VisitNode Empty = MakeLeaf(RegionType.Empty);

    public static VisitNode MakeLeaf(RegionType type) => new((uint)type << 2);

    public static VisitNode MakeLeaf(
        RegionType sw,
        RegionType se,
        RegionType nw,
        RegionType ne) => new(
        (uint)sw << 11 |
        (uint)se << 8 |
        (uint)nw << 5 |
        (uint)ne << 2 |
        0b10);

    public static VisitNode MakeBranch(int childIndex) =>
        new((uint)childIndex << 1 | 1);

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

    public RegionType RegionType => (RegionType)(Encoded >> 2 & 0b111);

    public int ChildIndex => (int)(Encoded >> 1);
    public int GetChildIndex(Quadrant quadrant) => ChildIndex + (int)quadrant;

    public RegionType SW => (RegionType)(Encoded >> 11 & 0b111);
    public RegionType SE => (RegionType)(Encoded >> 8 & 0b111);
    public RegionType NW => (RegionType)(Encoded >> 5 & 0b111);
    public RegionType NE => (RegionType)(Encoded >> 2 & 0b111);

    public RegionType GetQuadrant(Quadrant quadrant)
    {
        var offset = QuadrantOffset(quadrant);
        return (RegionType)(Encoded >> offset & 0b111);
    }

    public VisitNode WithSW(RegionType sw) => new(Encoded | ((uint)sw << 11) | 0b10);
    public VisitNode WithSE(RegionType se) => new(Encoded | ((uint)se << 8) | 0b10);
    public VisitNode WithNW(RegionType nw) => new(Encoded | ((uint)nw << 5) | 0b10);
    public VisitNode WithNE(RegionType ne) => new(Encoded | ((uint)ne << 2) | 0b10);

    public VisitNode WithQuadrant(Quadrant quadrant, RegionType type)
    {
        var offset = QuadrantOffset(quadrant);
        return new(Encoded | ((uint)type << offset) | 0b10);
    }

    private static int QuadrantOffset(Quadrant quadrant) => 11 - 3 * (int)quadrant;

    public override string ToString() => NodeType + NodeType switch
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