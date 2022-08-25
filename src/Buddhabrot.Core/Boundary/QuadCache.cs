namespace Buddhabrot.Core.Boundary;

public sealed class QuadCache
{
    private readonly List<Quad> _nodes;
    private readonly Dictionary<(Quad SW, Quad NW, Quad NE, Quad SE), Quad> _dict = new();

    public QuadCache(List<Quad> nodes) => _nodes = nodes;

    public int Size => _dict.Count;
    public int NumCachedValuesUsed { get; private set; }

    public Quad MakeQuad(Quad sw, Quad nw, Quad ne, Quad se)
    {
        if (sw.IsLeaf &&
            sw == nw &&
            nw == ne &&
            ne == se)
        {
            return sw;
        }

        var types = new[]
        {
            sw.Type,
            nw.Type,
            ne.Type,
            se.Type,
        };

        var (numBorders, numFilaments) = types.Aggregate(
            (Borders: 0, Filaments: 0),
            (count, type) => type switch
            {
                RegionType.Border => (count.Borders + 1, count.Filaments),
                RegionType.Filament => (count.Borders, count.Filaments + 1),
                _ => count
            });

        var type = numBorders >= numFilaments ? RegionType.Border : RegionType.Filament;

        var key = (sw, nw, ne, se);
        if (!_dict.TryGetValue(key, out var quad))
        {
            var index = _nodes.Count;
            quad = new Quad(type, index);
            _nodes.Add(sw);
            _nodes.Add(nw);
            _nodes.Add(ne);
            _nodes.Add(se);

            _dict.Add(key, quad);
        }
        else
        {
            NumCachedValuesUsed++;
        }

        return quad;
    }
}