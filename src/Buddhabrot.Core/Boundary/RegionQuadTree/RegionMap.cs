using System.Diagnostics;
using Buddhabrot.Core.ExtensionMethods.Collections;

namespace Buddhabrot.Core.Boundary.RegionQuadTree;

public sealed class RegionMap
{
    private readonly Quad _top;
    private readonly ComplexArea _topLevelArea = new(new Range(-2, 2), new Range(-2, 2));

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

        Dictionary<RegionId, Quad> quads = new();
        int maxX = 0;
        int maxY = 0;
        foreach (var region in regions)
        {
            quads.Add(region, Quad.Border);
            maxX = Math.Max(maxX, region.X);
            maxY = Math.Max(maxY, region.Y);
        }

        var sideLength = 2.0 / (1 << verticalPower);
        PopulatedArea = new ComplexArea(
            Range.FromMinAndLength(-2, maxX * sideLength), 
            new Range(-maxY * sideLength, 0));
        
        Dictionary<RegionId, Quad> nextLevel = new();

        for (int level = verticalPower; level > 0; level--)
        {
            while (quads.Any())
            {
                var nextLevelPosition = quads.Keys.First().Halve();

                var levelTopLeft = nextLevelPosition.Double();
                var nw = quads.RemoveOr(levelTopLeft, Quad.Empty);
                var ne = quads.RemoveOr(levelTopLeft.MoveRight(), Quad.Empty);
                var se = quads.RemoveOr(levelTopLeft.MoveRight().MoveDown(), Quad.Empty);
                var sw = quads.RemoveOr(levelTopLeft.MoveDown(), Quad.Empty);

                nextLevel.Add(nextLevelPosition, cache.MakeQuad(nw, ne, se, sw));
            }

            quads = nextLevel;
            nextLevel = new Dictionary<RegionId, Quad>();
        }

        log?.Invoke($"Cache size: {cache.Size:N0}, num times cached value used: {cache.NumCachedValuesUsed:N0}");

        // We should end up with only two quads
        Debug.Assert(quads.Count == 2);

        _top = cache.MakeQuad(
            nw: Quad.Empty,
            ne: Quad.Empty,
            se: quads[new RegionId(1, 0)],
            sw: quads[new RegionId(0, 0)]);
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
                    toCheck.Enqueue((quadArea.GetNW(), currentQuad.NW));
                    toCheck.Enqueue((quadArea.GetNE(), currentQuad.NE));
                    toCheck.Enqueue((quadArea.GetSE(), currentQuad.SE));
                    toCheck.Enqueue((quadArea.GetSW(), currentQuad.SW));
                }
            }
        }

        return visibleAreas;
    }
}