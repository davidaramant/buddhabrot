namespace Buddhabrot.Core.Boundary.RegionQuadTree;

sealed class QuadCache
{
    private readonly Dictionary<(Quad, Quad, Quad, Quad), Quad> _dict = new();
    public int Size => _dict.Count;
    public int NumCachedValuesUsed { get; private set; }

    public Quad MakeQuad(Quad nw, Quad ne, Quad se, Quad sw)
    {
        if (nw == ne &&
            ne == se &&
            se == sw)
        {
            if (nw == Quad.Empty)
                return Quad.Empty;

            if (nw == Quad.Border)
                return Quad.Border;
        }

        var key = (nw, ne, se, sw);
        if (!_dict.TryGetValue(key, out var cachedQuad))
        {
            cachedQuad = new Quad(nw, ne, se, sw);
            _dict.Add(key, cachedQuad);
        }
        else
        {
            NumCachedValuesUsed++;
        }

        return cachedQuad;
    }
}