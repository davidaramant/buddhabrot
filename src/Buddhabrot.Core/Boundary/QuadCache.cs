namespace Buddhabrot.Core.Boundary;

public sealed class QuadCache
{
    private readonly List<Quad> _nodes;
    private readonly Dictionary<(Quad SW, Quad NW, Quad NE, Quad SE), Quad> _dict = new();

    public QuadCache(List<Quad> nodes) => _nodes = nodes;

    public int Size => _dict.Count;
    public int NumCachedValuesUsed { get; private set; }

    public Quad MakeQuad(Quad sw, Quad se, Quad nw, Quad ne)
    {
        if (sw.IsLeaf &&
            sw == se &&
            se == nw &&
            nw == ne)
        {
            return sw;
        }

        var key = (sw, se, nw, ne);
        if (!_dict.TryGetValue(key, out var quad))
        {
            var type = new TypeCount()
                .Count(sw.Type)
                .Count(se.Type)
                .Count(nw.Type)
                .Count(ne.Type)
                .DetermineType();
            
            var index = _nodes.Count;
            quad = new Quad(type, index);
            _nodes.Add(sw);
            _nodes.Add(se);
            _nodes.Add(nw);
            _nodes.Add(ne);

            _dict.Add(key, quad);
        }
        else
        {
            NumCachedValuesUsed++;
        }

        return quad;
    }

    private readonly struct TypeCount
    {
        private readonly int _borderCount = 0;
        private readonly int _filamentCount = 0;
        private readonly int _rejectedCount = 0;

        public TypeCount()
        {
        }

        private TypeCount(int borderCount, int filamentCount, int rejectedCount)
        {
            _borderCount = borderCount;
            _filamentCount = filamentCount;
            _rejectedCount = rejectedCount;
        }

        public TypeCount Count(RegionType type) =>
            type switch
            {
                RegionType.Border => new(_borderCount + 1, _filamentCount, _rejectedCount),
                RegionType.Filament => new(_borderCount, _filamentCount + 1, _rejectedCount),
                RegionType.Rejected => new(_borderCount, _filamentCount, _rejectedCount + 1),
                _ => this,
            };

        public RegionType DetermineType()
        {
            if (_borderCount == 0 && _filamentCount == 0 && _rejectedCount == 0)
                return RegionType.Unknown;

            if (_borderCount >= _filamentCount && _borderCount >= _rejectedCount)
                return RegionType.Border;

            return _filamentCount >= _rejectedCount ? RegionType.Filament : RegionType.Rejected;
        }
    }
}