using System;

namespace Buddhabrot.Edges
{
    [Flags]
    public enum Corners : byte
    {
        None = 0,
        TopLeft = 1 << 0,
        TopRight = 1 << 1,
        BottomLeft = 1 << 2,
        BottomRight = 1 << 3,
        All = TopLeft | TopRight | BottomLeft | BottomRight,
    }
}
