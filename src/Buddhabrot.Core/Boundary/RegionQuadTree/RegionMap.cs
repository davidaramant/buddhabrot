namespace Buddhabrot.Core.Boundary.RegionQuadTree;

public sealed class RegionMap
{
    private readonly Quad _top;
    private readonly ComplexArea _topLevelArea = new(new Range(-2, 2), new Range(0, 4));

    public ComplexArea PopulatedArea { get; }

    public static readonly RegionMap Empty = new();

    private RegionMap()
    {
        _top = Quad.Empty;
        PopulatedArea = ComplexArea.Empty;
    }

    public RegionMap(int verticalPower, IEnumerable<RegionId> regions, Action<string>? log = default)
    {
        QuadCache cache = new();

        HashSet<RegionId> regionLookup = new();

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
            Range.FromMinAndLength(-2, maxX * sideLength),
            new Range(0, maxY * sideLength));


        Quad BuildQuad(int level, int xOffset, int yOffset)
        {
            if (level == -1)
            {
                return LookupLocation(xOffset, yOffset);
            }

            var levelWidth = 1 << level;

            return cache.MakeQuad(
                sw: BuildQuad(level - 1, xOffset, yOffset),
                se: BuildQuad(level - 1, xOffset + levelWidth, yOffset),
                ne: BuildQuad(level - 1, xOffset + levelWidth, yOffset + levelWidth),
                nw: BuildQuad(level - 1, xOffset, yOffset + levelWidth));
        }

        _top = BuildQuad(verticalPower, 0, 0);
        log?.Invoke($"Cache size: {cache.Size:N0}, num times cached value used: {cache.NumCachedValuesUsed:N0}");
    }

    public IReadOnlyList<ComplexArea> GetVisibleAreas(ComplexArea searchArea)
    {
        var visibleAreas = new List<ComplexArea>();

        var toCheck = new Queue<(ComplexArea, Quad)>();
        toCheck.Enqueue((_topLevelArea, _top));

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
                    toCheck.Enqueue((quadArea.GetNWQuadrant(), currentQuad.NW));
                    toCheck.Enqueue((quadArea.GetNEQuadrant(), currentQuad.NE));
                    toCheck.Enqueue((quadArea.GetSEQuadrant(), currentQuad.SE));
                    toCheck.Enqueue((quadArea.GetSWQuadrant(), currentQuad.SW));
                }
            }
        }

        return visibleAreas;
    }
}