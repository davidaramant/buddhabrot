namespace Buddhabrot.Core.Boundary;

public sealed class RegionLookup
{
    private readonly List<Quad> _nodes = new();

    private readonly ComplexArea _topLevelArea = new(
        new Range(-2, 2),
        new Range(0, 4));

    private enum Quadrant
    {
        SouthWest,
        NorthWest,
        NorthEast,
        SouthEast,
    }

    public static readonly RegionLookup Empty = new();
    public ComplexArea PopulatedArea { get; }

    private RegionLookup()
    {
        _nodes.Add(Quad.Empty);
        PopulatedArea = ComplexArea.Empty;
    }

    public RegionLookup(
        int verticalPower,
        IReadOnlyList<RegionId> regions,
        Action<string>? log = default)
    {
        QuadCache cache = new(_nodes);

        HashSet<RegionId> regionLookup = new(regions.Count);

        Quad LookupLocation(int x, int y) =>
            regionLookup.Contains(new RegionId(x, y)) ? Quad.Border : Quad.Empty;

        int maxX = 0;
        int maxY = 0;
        foreach (var region in regions)
        {
            regionLookup.Add(region);
            maxX = Math.Max(maxX, region.X);
            maxY = Math.Max(maxY, region.Y);
        }

        var sideLength = 2.0 / (1 << verticalPower);
        PopulatedArea = new ComplexArea(
            Range.FromMinAndLength(-2, (maxX + 1) * sideLength),
            new Range(0, (maxY + 1) * sideLength));

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
            sw: BuildQuad(verticalPower - 1, 0, 0),
            nw: Quad.Empty,
            ne: Quad.Empty,
            se: BuildQuad(verticalPower - 1, topLevelWidth, 0)));
        log?.Invoke($"Cache size: {cache.Size:N0}, num times cached value used: {cache.NumCachedValuesUsed:N0}");
    }

    public IReadOnlyList<ComplexArea> GetVisibleAreas(ComplexArea searchArea)
    {
        var visibleAreas = new List<ComplexArea>();

        var toCheck = new Queue<(ComplexArea, Quad)>();
        toCheck.Enqueue((_topLevelArea, _nodes.Last()));

        while (toCheck.Any())
        {
            var (quadArea, currentQuad) = toCheck.Dequeue();

            if (!currentQuad.IsEmpty &&
                searchArea.OverlapsWith(quadArea))
            {
                if (currentQuad.IsBorder)
                {
                    visibleAreas.Add(quadArea.Intersect(searchArea));
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

        return visibleAreas;
    }

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
                if (sw.IsEmpty)
                    return Quad.Empty;

                if (sw.IsBorder)
                    return Quad.Border;
            }

            var key = (sw, nw, ne, se);
            if (!_dict.TryGetValue(key, out var quad))
            {
                var index = _nodes.Count;
                quad = new Quad(index);
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

    readonly record struct Quad(int ChildIndex)
    {
        public static readonly Quad Empty = new(-1);
        public static readonly Quad Border = new(-2);

        public bool IsEmpty => ChildIndex == -1;
        public bool IsBorder => ChildIndex == -2;

        public int GetQuadrantIndex(Quadrant child) => ChildIndex + (int) child;

        public override string ToString()
        {
            if (IsEmpty) return "Empty";
            if (IsBorder) return "Border";
            return ChildIndex.ToString();
        }
    }
}