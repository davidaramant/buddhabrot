namespace Buddhabrot.Core.Boundary.RegionQuadTree;

sealed class Quad
{
    public static readonly Quad Empty = new(QuadType.Empty);
    public static readonly Quad Border = new(QuadType.Border);

    public QuadType Type { get; }

    public Quad NW { get; }
    public Quad NE { get; }
    public Quad SE { get; }
    public Quad SW { get; }

    public override string ToString() => Type.ToString();

    private Quad(QuadType type)
    {
        Type = type;
        NW = default!;
        NE = default!;
        SE = default!;
        SW = default!;
    }

    public Quad(Quad nw, Quad ne, Quad se, Quad sw)
    {
        Type = QuadType.Mixed;
        NW = nw;
        NE = ne;
        SE = se;
        SW = sw;
    }
}