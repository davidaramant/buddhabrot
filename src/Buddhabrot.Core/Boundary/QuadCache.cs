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

        var key = (sw, nw, ne, se);
        if (!_dict.TryGetValue(key, out var quad))
        {
            var type = new TypeCount()
                .Count(sw.Type)
                .Count(nw.Type)
                .Count(ne.Type)
                .Count(se.Type)
                .DetermineType();
            
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

    private readonly struct TypeCount
    {
        private readonly int _borderCount = 0;
        private readonly int _filamentCount = 0;
        private readonly int _inSetCount = 0;

        public TypeCount()
        {
        }

        private TypeCount(int borderCount, int filamentCount, int inSetCount)
        {
            _borderCount = borderCount;
            _filamentCount = filamentCount;
            _inSetCount = inSetCount;
        }

        public TypeCount Count(RegionType type) =>
            type switch
            {
                RegionType.Border => new(_borderCount + 1, _filamentCount, _inSetCount),
                RegionType.Filament => new(_borderCount, _filamentCount + 1, _inSetCount),
                RegionType.InSet => new(_borderCount, _filamentCount, _inSetCount + 1),
                _ => this,
            };

        public RegionType DetermineType()
        {
            if (_borderCount == 0 && _filamentCount == 0 && _inSetCount == 0)
                return RegionType.Empty;

            if (_borderCount >= 2)
                return RegionType.Border;

            return _filamentCount >= _inSetCount ? RegionType.Filament : RegionType.InSet;
        }
    }
}