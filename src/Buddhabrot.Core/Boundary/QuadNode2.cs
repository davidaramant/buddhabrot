namespace Buddhabrot.Core.Boundary;

public static class QuadNode2
{
    public static readonly uint UnknownLeaf = MakeLeaf(RegionType.Unknown);

    public static uint MakeLeaf(RegionType type) => (uint)type << 2;

    public static uint MakeLeaf(
        RegionType type,
        RegionType ll,
        RegionType lr,
        RegionType ul,
        RegionType ur) =>
        (uint)ll << 10 |
        (uint)lr << 8 |
        (uint)ul << 6 |
        (uint)ur << 4 |
        (uint)type << 2 |
        (uint)NodeType.LeafQuad;

    public static uint MakeBranch(RegionType type, int childIndex) =>
        (uint)childIndex << 4 | (uint)type << 2 | (uint)NodeType.Branch;
    
    public static NodeType GetNodeType(this uint encoded) => (NodeType) (encoded & 0b11);
    public static RegionType GetRegionType(this uint encoded) => (RegionType) (encoded >> 2 & 0b11);

    public static int GetChildIndex(this uint encoded) => (int) (encoded >> 4);
    public static int GetChildIndex(this uint encoded, Quadrant quadrant) => encoded.GetChildIndex() + (int) quadrant;

    public static RegionType GetLL(this uint encoded) => (RegionType) (encoded >> 10 & 0b11);
    public static RegionType GetLR(this uint encoded) => (RegionType) (encoded >> 8 & 0b11);
    public static RegionType GetUL(this uint encoded) => (RegionType) (encoded >> 6 & 0b11);
    public static RegionType GetUR(this uint encoded) => (RegionType) (encoded >> 4 & 0b11);

    public static RegionType GetQuadrant(this uint encoded, Quadrant quadrant)
    {
        var offset = QuadrantOffset(quadrant);
        return (RegionType) (encoded >> offset & 0b11);
    }

    public static uint WithLL(this uint encoded, RegionType ll) => encoded | (((uint) ll << 10) + (int) NodeType.LeafQuad);
    public static uint WithLR(this uint encoded, RegionType lr) => encoded | (((uint) lr << 8) + (int) NodeType.LeafQuad);
    public static uint WithUL(this uint encoded, RegionType ul) => encoded | (((uint) ul << 6) + (int) NodeType.LeafQuad);
    public static uint WithUR(this uint encoded, RegionType ur) => encoded | (((uint) ur << 4) + (int) NodeType.LeafQuad);

    public static uint WithQuadrant(this uint encoded, Quadrant quadrant, RegionType type)
    {
        var offset = QuadrantOffset(quadrant);
        return encoded | ((uint) type << offset) + (int) NodeType.LeafQuad;
    }

    private static int QuadrantOffset(Quadrant quadrant) => 10 - 2 * (int) quadrant;

    public static string ToDisplayString(this uint encoded) => $"{encoded.GetNodeType()} ({encoded.GetRegionType()})" + encoded.GetNodeType() switch
    {
        NodeType.Leaf => string.Empty,
        NodeType.Branch => $" {encoded.GetChildIndex()}",
        NodeType.LeafQuad => $" LL:{encoded.GetLL()} LR:{encoded.GetLR()} UL:{encoded.GetUL()} UR:{encoded.GetUR()}",
        _ => "how could this have happened"
    };
}