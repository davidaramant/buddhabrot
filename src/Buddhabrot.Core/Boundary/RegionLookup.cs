namespace Buddhabrot.Core.Boundary;

public sealed class RegionLookup
{
    private readonly List<Quad> _nodes = new();

    private readonly ComplexArea _topLevelArea = new(
        new Interval(-2, 2),
        new Interval(-2, 2));

    public static readonly RegionLookup Empty = new();
    public int MaxX { get; }
    public int MaxY { get; }
    public ComplexArea PopulatedArea { get; }

    private RegionLookup()
    {
        _nodes.Add(Quad.Empty);
        MaxX = 0;
        MaxY = 0;
        PopulatedArea = ComplexArea.Empty;
    }

    public RegionLookup(
        int verticalPower,
        int maxX,
        int maxY,
        IEnumerable<uint> rawNodes)
    {
        MaxX = maxX;
        MaxY = maxY;
        _nodes = rawNodes.Select(i => new Quad(i)).ToList();
        PopulatedArea = ComputePopulatedArea(verticalPower, maxX, maxY);
    }

    public RegionLookup(
        int verticalPower,
        IReadOnlyList<(RegionId, RegionType)> regions,
        Action<string>? log = default)
    {
        QuadCache cache = new(_nodes);

        Dictionary<RegionId, Quad> regionLookup = new(regions.Count);

        Quad LookupLocation(int x, int y) =>
            regionLookup.TryGetValue(new RegionId(x, y), out var quad) ? quad : Quad.Empty;

        int maxX = 0;
        int maxY = 0;
        foreach (var (region, type) in regions)
        {
            regionLookup.Add(region, type switch
            {
                RegionType.Border => Quad.Border,
                RegionType.Filament => Quad.Filament,
                _ => throw new InvalidOperationException("What type did I get?? " + type)
            });
            maxX = Math.Max(maxX, region.X);
            maxY = Math.Max(maxY, region.Y);
        }

        MaxX = maxX;
        MaxY = maxY;
        PopulatedArea = ComputePopulatedArea(verticalPower, maxX, maxY);

        Quad BuildQuad(int level, int xOffset, int yOffset)
        {
            if (level == 0)
            {
                return cache.MakeQuad(
                    sw: LookupLocation(xOffset, yOffset),
                    nw: LookupLocation(xOffset, yOffset + 1),
                    ne: LookupLocation(xOffset + 1, yOffset + 1),
                    se: LookupLocation(xOffset + 1, yOffset));
            }

            var levelWidth = 1 << level;

            return cache.MakeQuad(
                sw: BuildQuad(level - 1, xOffset, yOffset),
                nw: BuildQuad(level - 1, xOffset, yOffset + levelWidth),
                ne: BuildQuad(level - 1, xOffset + levelWidth, yOffset + levelWidth),
                se: BuildQuad(level - 1, xOffset + levelWidth, yOffset));
        }

        var topLevelWidth = 1 << verticalPower;
        _nodes.Add(cache.MakeQuad(
            sw: Quad.Empty,
            nw: BuildQuad(verticalPower - 1, 0, 0),
            ne: BuildQuad(verticalPower - 1, topLevelWidth, 0),
            se: Quad.Empty));
        log?.Invoke(
            $"Cache size: {cache.Size:N0}, num times cached value used: {cache.NumCachedValuesUsed:N0}, Num nodes: {_nodes.Count:N0}");
    }

    private static ComplexArea ComputePopulatedArea(int verticalPower, int maxX, int maxY)
    {
        var sideLength = 2.0 / (1 << verticalPower);
        return new ComplexArea(
            Interval.FromMinAndLength(-2, (maxX + 1) * sideLength),
            new Interval(0, (maxY + 1) * sideLength));
    }

    public IReadOnlyList<(ComplexArea Area, RegionType Type)> GetVisibleAreas(ComplexArea searchArea,
        double minVisibleWidth)
    {
        var visibleAreas = new List<(ComplexArea, RegionType)>();

        var toCheck = new Queue<(ComplexArea, Quad)>();
        toCheck.Enqueue((_topLevelArea, _nodes.Last()));

        while (toCheck.Any())
        {
            var (quadArea, currentQuad) = toCheck.Dequeue();

            if (!currentQuad.IsEmptyLeaf &&
                searchArea.OverlapsWith(quadArea))
            {
                var nextWidth = quadArea.Width / 2d;

                if (currentQuad.IsBorderLeaf || currentQuad.IsFilamentLeaf || nextWidth < minVisibleWidth)
                {
                    visibleAreas.Add((quadArea.Intersect(searchArea), currentQuad.Type));
                }
                else
                {
                    toCheck.Enqueue(
                        (quadArea.GetSWQuadrant(), _nodes[currentQuad.GetQuadrantIndex(Quadrant.SouthWest)]));
                    toCheck.Enqueue(
                        (quadArea.GetNWQuadrant(), _nodes[currentQuad.GetQuadrantIndex(Quadrant.NorthWest)]));
                    toCheck.Enqueue(
                        (quadArea.GetNEQuadrant(), _nodes[currentQuad.GetQuadrantIndex(Quadrant.NorthEast)]));
                    toCheck.Enqueue(
                        (quadArea.GetSEQuadrant(), _nodes[currentQuad.GetQuadrantIndex(Quadrant.SouthEast)]));
                }
            }
        }

        // Check the mirrored values to get build the bottom of the set
        toCheck.Enqueue((_topLevelArea, _nodes.Last()));

        while (toCheck.Any())
        {
            var (quadArea, currentQuad) = toCheck.Dequeue();

            if (!currentQuad.IsEmptyLeaf &&
                searchArea.OverlapsWith(quadArea))
            {
                var nextWidth = quadArea.Width / 2d;

                if (currentQuad.IsBorderLeaf || currentQuad.IsFilamentLeaf || nextWidth < minVisibleWidth)
                {
                    visibleAreas.Add((quadArea.Intersect(searchArea), currentQuad.Type));
                }
                else
                {
                    toCheck.Enqueue(
                        (quadArea.GetNWQuadrant(), _nodes[currentQuad.GetQuadrantIndex(Quadrant.SouthWest)]));
                    toCheck.Enqueue(
                        (quadArea.GetSWQuadrant(), _nodes[currentQuad.GetQuadrantIndex(Quadrant.NorthWest)]));
                    toCheck.Enqueue(
                        (quadArea.GetSEQuadrant(), _nodes[currentQuad.GetQuadrantIndex(Quadrant.NorthEast)]));
                    toCheck.Enqueue(
                        (quadArea.GetNEQuadrant(), _nodes[currentQuad.GetQuadrantIndex(Quadrant.SouthEast)]));
                }
            }
        }

        return visibleAreas;
    }

    public uint[] GetRawNodes() => _nodes.Select(n => n.Encoded).ToArray();

    sealed class QuadCache
    {
        private readonly List<Quad> _nodes;
        private readonly Dictionary<(Quad NW, Quad NE, Quad SE, Quad SW), Quad> _dict = new();

        public QuadCache(List<Quad> nodes) => _nodes = nodes;

        public int Size => _dict.Count;
        public int NumCachedValuesUsed { get; private set; }

        public Quad MakeQuad(Quad sw, Quad nw, Quad ne, Quad se)
        {
            if (sw == nw &&
                nw == ne &&
                ne == se)
            {
                return sw.Type switch
                {
                    RegionType.Border => Quad.Border,
                    RegionType.Empty => Quad.Empty,
                    RegionType.Filament => Quad.Filament,
                    _ => throw new Exception("Unknown region type")
                };
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
}