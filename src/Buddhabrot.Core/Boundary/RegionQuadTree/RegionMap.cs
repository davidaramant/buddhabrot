using System.Diagnostics;
using Buddhabrot.Core.ExtensionMethods.Collections;

namespace Buddhabrot.Core.Boundary.RegionQuadTree;

public sealed class RegionMap
{
    private readonly Quad _top;
    private const int _widthOfQuarter = 2;

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

        log?.Invoke($"Cache count: {cache.Count}");

        // We should end up with only two quads
        Debug.Assert(quads.Count == 2);

        _top = cache.Transform((Quad.Empty, Quad.Empty, quads[new RegionId(0, 0)], quads[new RegionId(1, 0)]));
    }

    public IEnumerable<ComplexArea> GetIntersectingRegions(ComplexArea searchArea)
    {
        throw new NotImplementedException();
    }
}