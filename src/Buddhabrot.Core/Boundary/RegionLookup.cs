using System.Drawing;

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

    private RegionLookup()
    {
        _nodes.Add(Quad.UnknownLeaf);
        Levels = 1;
    }

    public RegionLookup(
        int levels,
        IEnumerable<uint> rawNodes)
    {
        Levels = levels;
        _nodes = rawNodes.Select(i => new Quad(i)).ToList();
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
            regionLookup.TryGetValue(new RegionId(x, y), out var quad) ? quad : Quad.UnknownLeaf;

        foreach (var (region, type) in regions)
        {
            regionLookup.Add(region, type switch
            {
                RegionType.Border => Quad.BorderLeaf,
                RegionType.Filament => Quad.FilamentLeaf,
                RegionType.Rejected => Quad.RejectedLeaf,
                _ => throw new InvalidOperationException("What type did I get?? " + type)
            });
        }

        Quad BuildQuad(int depth, int x, int y, int xOffset = 0)
        {
            if (depth == Levels - 1)
            {
                return LookupLocation(x + xOffset, y);
            }

            var newX = x << 1;
            var newY = y << 1;

            return cache.MakeQuad(
                sw: BuildQuad(depth + 1, newX, newY, xOffset),
                se: BuildQuad(depth + 1, newX + 1, newY, xOffset), 
                nw: BuildQuad(depth + 1, newX, newY + 1, xOffset), 
                ne: BuildQuad(depth + 1, newX + 1, newY + 1, xOffset));
        }

        _nodes.Add(cache.MakeQuad(
            sw: Quad.UnknownLeaf,
            se: Quad.UnknownLeaf, 
            nw: BuildQuad(1, 0, 0), 
            ne: BuildQuad(1, 0, 0, xOffset: divisions.QuadrantDivisions)));
        log?.Invoke(
            $"Cache size: {cache.Size:N0}, num times cached value used: {cache.NumCachedValuesUsed:N0}, Num nodes: {_nodes.Count:N0}");
    }

    public IReadOnlyList<(Rectangle Area, RegionType Type)> GetVisibleAreas(
        SquareBoundary bounds,
        IEnumerable<Rectangle> searchAreas)
    {
        var visibleAreas = new List<(Rectangle, RegionType)>();

        foreach (var searchArea in searchAreas)
        {
            var toCheck = new Queue<(SquareBoundary, Quad)>();
            toCheck.Enqueue((bounds, _nodes.Last()));

            while (toCheck.Any())
            {
                var (quadArea, currentQuad) = toCheck.Dequeue();

                if (currentQuad.IsUnknownLeaf)
                    continue;

                var intersection = quadArea.IntersectWith(searchArea);
                if (intersection == Rectangle.Empty)
                    continue;

                if (currentQuad.IsLeaf || quadArea.IsPoint)
                {
                    visibleAreas.Add((intersection, currentQuad.Type));
                }
                else
                {
                    toCheck.Enqueue(
                        (quadArea.GetSWQuadrant(), _nodes[currentQuad.GetQuadrantIndex(Quadrant.SouthWest)]));
                    toCheck.Enqueue(
                        (quadArea.GetSEQuadrant(), _nodes[currentQuad.GetQuadrantIndex(Quadrant.SouthEast)]));
                    toCheck.Enqueue(
                        (quadArea.GetNWQuadrant(), _nodes[currentQuad.GetQuadrantIndex(Quadrant.NorthWest)]));
                    toCheck.Enqueue(
                        (quadArea.GetNEQuadrant(), _nodes[currentQuad.GetQuadrantIndex(Quadrant.NorthEast)]));
                }
            }

            // Check the mirrored values to build the bottom of the set
            toCheck.Enqueue((bounds, _nodes.Last()));

            while (toCheck.Any())
            {
                var (quadArea, currentQuad) = toCheck.Dequeue();

                if (currentQuad.IsUnknownLeaf)
                    continue;

                var intersection = quadArea.IntersectWith(searchArea);
                if (intersection == Rectangle.Empty)
                    continue;

                if (currentQuad.IsLeaf || quadArea.IsPoint)
                {
                    visibleAreas.Add((intersection, currentQuad.Type));
                }
                else
                {
                    toCheck.Enqueue(
                        (quadArea.GetNWQuadrant(), _nodes[currentQuad.GetQuadrantIndex(Quadrant.SouthWest)]));
                    toCheck.Enqueue(
                        (quadArea.GetNEQuadrant(), _nodes[currentQuad.GetQuadrantIndex(Quadrant.SouthEast)]));
                    toCheck.Enqueue(
                        (quadArea.GetSWQuadrant(), _nodes[currentQuad.GetQuadrantIndex(Quadrant.NorthWest)]));
                    toCheck.Enqueue(
                        (quadArea.GetSEQuadrant(), _nodes[currentQuad.GetQuadrantIndex(Quadrant.NorthEast)]));
                }
            }
        }

        return visibleAreas;
    }

    public IReadOnlyList<Quad> GetRawNodes() => _nodes;
}