namespace Buddhabrot.Core.Boundary.RegionQuadTree;

public sealed class RegionMap2 : IRegionMap
{
    private readonly List<Quad> _nodes = new() {Quad.Empty};

    private readonly ComplexArea _topLevelArea = new(
        new Range(-2, 2),
        new Range(0, 4));

    private enum Quadrant
    {
        SouthWest,
        SouthEast,
        NorthEast,
        NorthWest,
    }

    public static readonly RegionMap2 Empty = new();
    public ComplexArea PopulatedArea { get; }

    private RegionMap2()
    {
        PopulatedArea = ComplexArea.Empty;
    }

    public RegionMap2(
        int verticalPower,
        IReadOnlyList<RegionId> regions,
        Action<string>? log = default)
    {
        int maxX = 0;
        int maxY = 0;

        // TODO: This is awful. Write an insert method and test it, then use that in the constructor
        
        foreach (var region in regions)
        {
            maxX = Math.Max(maxX, region.X);
            maxY = Math.Max(maxY, region.Y);

            var quadIndex = 0;
            var quad = _nodes[quadIndex];

            int xOffset = 0;
            int yOffset = 0;
            for (int level = verticalPower; level >= 0; level--)
            {
                if (quad.Type == QuadType.Empty)
                {
                    int index = _nodes.Count;
                    _nodes.Add(Quad.Empty);
                    _nodes.Add(Quad.Empty);
                    _nodes.Add(Quad.Empty);
                    _nodes.Add(Quad.Empty);
                    quad = new Quad(index);
                    _nodes[quadIndex] = quad;
                }

                // figure out which quadrant it's in
                var halfWidth = 1 << level;
                var xSmall = region.X < (xOffset + halfWidth);
                var ySmall = region.Y < (yOffset + halfWidth);
                var quadrant = (xSmall, ySmall) switch
                {
                    (true, true) => Quadrant.SouthWest,
                    (false, true) => Quadrant.SouthEast,
                    (true, false) => Quadrant.NorthWest,
                    (false, false) => Quadrant.NorthEast,
                };

                switch (quadrant)
                {
                    case Quadrant.NorthEast:
                        xOffset += halfWidth;
                        yOffset += halfWidth;
                        break;
                    case Quadrant.SouthWest:
                        xOffset += halfWidth;
                        break;
                    case Quadrant.NorthWest:
                        yOffset += halfWidth;
                        break;
                    case Quadrant.SouthEast:
                        break;
                }
                quadIndex = quad.GetQuadrantIndex(quadrant);
                quad = _nodes[quadIndex];
            }

            _nodes[quadIndex] = Quad.Border;
        }

        var sideLength = 2.0 / (1 << verticalPower);
        PopulatedArea = new ComplexArea(
            Range.FromMinAndLength(-2, (maxX + 1) * sideLength),
            new Range(0, (maxY + 1) * sideLength));

        log?.Invoke($"Quad tree nodes: {_nodes.Count:N0}");
    }

    public IReadOnlyList<ComplexArea> GetVisibleAreas(ComplexArea searchArea)
    {
        var visibleAreas = new List<ComplexArea>();

        var toCheck = new Queue<(ComplexArea, Quad)>();
        toCheck.Enqueue((_topLevelArea, _nodes[0]));

        while (toCheck.Any())
        {
            var (quadArea, currentQuad) = toCheck.Dequeue();

            if (currentQuad != Quad.Empty &&
                searchArea.OverlapsWith(quadArea))
            {
                if (currentQuad == Quad.Border)
                {
                    visibleAreas.Add(quadArea.Intersect(searchArea));
                }
                else
                {
                    toCheck.Enqueue(
                        (quadArea.GetNWQuadrant(), _nodes[currentQuad.GetQuadrantIndex(Quadrant.NorthWest)]));
                    toCheck.Enqueue(
                        (quadArea.GetNEQuadrant(), _nodes[currentQuad.GetQuadrantIndex(Quadrant.NorthEast)]));
                    toCheck.Enqueue(
                        (quadArea.GetSEQuadrant(), _nodes[currentQuad.GetQuadrantIndex(Quadrant.SouthEast)]));
                    toCheck.Enqueue(
                        (quadArea.GetSWQuadrant(), _nodes[currentQuad.GetQuadrantIndex(Quadrant.SouthWest)]));
                }
            }
        }

        return visibleAreas;
    }

    sealed class QuadCache
    {
        private readonly List<Quad> _nodes;
        private readonly Dictionary<int, int> _dict = new();

        public QuadCache(List<Quad> nodes) => _nodes = nodes;

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

                if (nw == Quad.Filament)
                    return Quad.Filament;
            }

            var key = HashCode.Combine(nw, ne, se, sw);
            if (!_dict.TryGetValue(key, out var index))
            {
                index = _nodes.Count;
                _nodes.Add(new Quad(index));
                _nodes.Add(nw);
                _nodes.Add(ne);
                _nodes.Add(se);
                _nodes.Add(sw);

                _dict.Add(key, index);
            }
            else
            {
                NumCachedValuesUsed++;
            }

            return _nodes[index];
        }
    }

    readonly record struct Quad(int ChildIndex)
    {
        public static readonly Quad Empty = new(-1);
        public static readonly Quad Border = new(-2);
        public static readonly Quad Filament = new(-3);

        public QuadType Type => ChildIndex switch
        {
            -1 => QuadType.Empty,
            -2 => QuadType.Border,
            -3 => QuadType.Filament,
            _ => QuadType.Mixed
        };

        public int GetQuadrantIndex(Quadrant child) => ChildIndex + (int) child;
    }
}