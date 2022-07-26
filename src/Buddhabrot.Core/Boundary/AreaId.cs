﻿namespace Buddhabrot.Core.Boundary;

/// <summary>
/// Id of an area.
/// </summary>
/// <remarks>
/// Origin is the bottom-left (-2 + 0i)
/// </remarks>
public readonly record struct AreaId(int EncodedPosition)
{
    public int X => ushort.MaxValue & EncodedPosition;
    public int Y => EncodedPosition >> 16;

    public AreaId(int x, int y) : this((y << 16) + x)
    {
    }

    public override string ToString() => $"Area({X}, {Y})";
}