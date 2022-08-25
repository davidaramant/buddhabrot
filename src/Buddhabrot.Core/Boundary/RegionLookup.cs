namespace Buddhabrot.Core.Boundary;

public sealed class RegionLookup
{
    private readonly List<Quad> _nodes = new();
    public int Levels { get; }
    public int NodeCount => _nodes.Count;

    private readonly ComplexArea _topLevelArea = new(
        new Interval(-2, 2),
        new Interval(-2, 2));

    public static readonly RegionLookup Empty = new();
    public int MaxX { get; }
    public int MaxY { get; }
    public ComplexArea PopulatedArea { get; }

    private RegionLookup()
    {
        _nodes.Add(Quad.EmptyLeaf);
        Levels = 1;
        MaxX = 0;
        MaxY = 0;
        PopulatedArea = ComplexArea.Empty;
    }

    public RegionLookup(
        AreaDivisions divisions,
        int maxX,
        int maxY,
        IEnumerable<uint> rawNodes)
    {
        MaxX = maxX;
        MaxY = maxY;
        _nodes = rawNodes.Select(i => new Quad(i)).ToList();
        PopulatedArea = ComputePopulatedArea(divisions.VerticalPower, maxX, maxY);
    }

    public RegionLookup(
        AreaDivisions divisions,
        IReadOnlyList<(RegionId, RegionType)> regions,
        Action<string>? log = default)
    {
        Levels = divisions.VerticalPower + 2;
        QuadCache cache = new(_nodes);

        Dictionary<RegionId, Quad> regionLookup = new(regions.Count);

        Quad LookupLocation(int x, int y) =>
            regionLookup.TryGetValue(new RegionId(x, y), out var quad) ? quad : Quad.EmptyLeaf;

        int maxX = 0;
        int maxY = 0;
        foreach (var (region, type) in regions)
        {
            regionLookup.Add(region, type switch
            {
                RegionType.Border => Quad.BorderLeaf,
                RegionType.Filament => Quad.FilamentLeaf,
                _ => throw new InvalidOperationException("What type did I get?? " + type)
            });
            maxX = Math.Max(maxX, region.X);
            maxY = Math.Max(maxY, region.Y);
        }

        MaxX = maxX;
        MaxY = maxY;
        PopulatedArea = ComputePopulatedArea(divisions.VerticalPower, maxX, maxY);

        Quad BuildQuad(int depth, int x, int y, int xOffset = 0)
        {
            if (depth == Levels - 1)
            {
                return LookupLocation(x + xOffset, y);
            }

            var levelWidth = 1 << depth;
            var newX = x * levelWidth;
            var newY = x * levelWidth;

            return cache.MakeQuad(
                sw: BuildQuad(depth + 1, newX, newY, xOffset),
                nw: BuildQuad(depth + 1, newX, newY + 1, xOffset),
                ne: BuildQuad(depth + 1, newX + 1, newY + 1, xOffset),
                se: BuildQuad(depth + 1, newX + 1, newY, xOffset));
        }

        _nodes.Add(cache.MakeQuad(
            sw: Quad.EmptyLeaf,
            nw: BuildQuad(1, 0, 0),
            ne: BuildQuad(1, 0, 0, xOffset: divisions.QuadrantDivisions),
            se: Quad.EmptyLeaf));
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

                if (nextWidth < minVisibleWidth)
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

                if (nextWidth < minVisibleWidth)
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

    public IReadOnlyList<Quad> GetRawNodes() => _nodes;
}