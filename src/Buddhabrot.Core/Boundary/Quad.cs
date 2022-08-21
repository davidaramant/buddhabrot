namespace Buddhabrot.Core.Boundary;

public readonly record struct Quad(uint Encoded)
{
    private const int HasChildrenBits = 1;
    private const int TypeBits = 2;
    private const int MetadataBits = HasChildrenBits + TypeBits;
    private const int MetadataShift = 32 - MetadataBits;
    private const int IndexMask = ~(0b111 << MetadataShift);

    private Quad(RegionType type) : this((uint)type << MetadataShift)
    {
    }

    public Quad(RegionType type, int childIndex)
        : this((uint)((1 << 31) | ((uint)type << MetadataShift) | (uint)childIndex))
    {
    }

    public int ChildIndex => (int)(Encoded & IndexMask);
    public RegionType Type => (RegionType)((Encoded >> MetadataShift) & 0b11);
    public bool HasChildren => (Encoded >> 31) == 0b1;

    public static readonly Quad Empty = new(RegionType.Empty);
    public static readonly Quad Border = new(RegionType.Border);
    public static readonly Quad Filament = new(RegionType.Filament);

    public bool IsEmptyLeaf => this == Empty;
    public bool IsBorderLeaf => this == Border;
    public bool IsFilamentLeaf => this == Filament;

    public int GetQuadrantIndex(Quadrant child) => ChildIndex + (int)child;

    public override string ToString() => $"{Type} {(HasChildren ? ChildIndex : "Leaf")}";
}