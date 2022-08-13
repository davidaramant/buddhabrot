namespace Buddhabrot.Core.Boundary.RegionQuadTree;

public sealed class RegionMap2 : IRegionMap
{
    private readonly List<Quad> _nodes = new();
    private readonly Quad _top;
    private readonly ComplexArea _topLevelArea = new(new Range(-2, 2), new Range(0, 4));

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
                    se: LookupLocation(xOffset + 1, yOffset),
                    ne: LookupLocation(xOffset + 1, yOffset + 1),
                    nw: LookupLocation(xOffset, yOffset + 1));
            }

            var levelWidth = 1 << level;

            return cache.MakeQuad(
                sw: BuildQuad(level - 1, xOffset, yOffset),
                se: BuildQuad(level - 1, xOffset + levelWidth, yOffset),
                ne: BuildQuad(level - 1, xOffset + levelWidth, yOffset + levelWidth),
                nw: BuildQuad(level - 1, xOffset, yOffset + levelWidth));
        }

        var topLevelWidth = 1 << verticalPower;
        _top = cache.MakeQuad(
            nw: Quad.Empty,
            ne: Quad.Empty,
            sw: BuildQuad(verticalPower - 1, 0, 0),
            se: BuildQuad(verticalPower - 1, topLevelWidth, 0));
        log?.Invoke($"Cache size: {cache.Size:N0}, num times cached value used: {cache.NumCachedValuesUsed:N0}");
    }

    public IReadOnlyList<ComplexArea> GetVisibleAreas(ComplexArea searchArea)
    {
        return Array.Empty<ComplexArea>();
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
    }
}