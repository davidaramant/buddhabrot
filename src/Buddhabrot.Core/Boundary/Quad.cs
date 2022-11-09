﻿namespace Buddhabrot.Core.Boundary;

public readonly record struct Quad(uint Encoded)
{ 
    private Quad(RegionType type) : this((uint)type << 1)
    {
    }

    public Quad(RegionType type, int childIndex)
        : this((uint)childIndex << 3 | (uint)type << 1 | 1)
    {
    }

    public int ChildIndex => (int)(Encoded >> 3);
    public RegionType Type => (RegionType)((Encoded >> 1) & 0b11);
    public bool HasChildren => (Encoded & 1) == 1;
    public bool IsLeaf => !HasChildren;

    public static readonly Quad EmptyLeaf = new(RegionType.Empty);
    public static readonly Quad BorderLeaf = new(RegionType.Border);
    public static readonly Quad FilamentLeaf = new(RegionType.Filament);
    public static readonly Quad InSetLeaf = new(RegionType.InSet);

    public bool IsEmptyLeaf => this == EmptyLeaf;
    public bool IsBorderLeaf => this == BorderLeaf;
    public bool IsFilamentLeaf => this == FilamentLeaf;
    public bool IsInSetLeaf => this == InSetLeaf;

    public int GetQuadrantIndex(Quadrant child) => ChildIndex + (int)child;

    public override string ToString() => $"{Type} {(HasChildren ? ChildIndex : "Leaf")}";
}