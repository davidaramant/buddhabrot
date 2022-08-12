using System.Diagnostics;
using Buddhabrot.Core.ExtensionMethods.Collections;

namespace Buddhabrot.Core.Boundary.RegionQuadTree;

public sealed class RegionMap
{
    private readonly Quad _top;
    private readonly ComplexArea _topLevelArea = new(new Range(-2, 2), new Range(-2, 2));

    public static readonly RegionMap Empty = new();

    private RegionMap() => _top = Quad.Empty;

    public RegionMap(int verticalPower, IEnumerable<RegionId> regions, Action<string>? log = default)
    {
        TransformCache<(Quad NW, Quad NE, Quad SE, Quad SW), Quad> cache = new(quarters =>
            Quad.Make(nw: quarters.NW, ne: quarters.NE, se: quarters.SE, sw: quarters.SW));

        var quads = regions.ToDictionary(r => r, _ => Quad.Border);
        Dictionary<RegionId, Quad> nextLevel = new();

        for (int level = verticalPower; level > 0; level--)
        {
            while (quads.Any())
            {
                var nextLevelPosition = quads.Keys.First().Halve();

                var nw = quads.RemoveOr(nextLevelPosition.Double(), Quad.Empty);
                var ne = quads.RemoveOr(nextLevelPosition.Double().MoveRight(), Quad.Empty);
                var se = quads.RemoveOr(nextLevelPosition.Double().MoveRight().MoveDown(), Quad.Empty);
                var sw = quads.RemoveOr(nextLevelPosition.Double().MoveDown(), Quad.Empty);

                nextLevel.Add(nextLevelPosition, cache.Transform((nw, ne, se, sw)));
            }

            quads = nextLevel;
            nextLevel = new Dictionary<RegionId, Quad>();
        }

        log?.Invoke($"Cache size: {cache.Size:N0}, num times cached value used: {cache.NumCachedValuesUsed:N0}");

        // We should end up with only two quads
        Debug.Assert(quads.Count == 2);

        _top = cache.Transform((
                NW: Quad.Empty,
                NE: Quad.Empty,
                SE: quads[new RegionId(1, 0)],
                SW: quads[new RegionId(0, 0)]));
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