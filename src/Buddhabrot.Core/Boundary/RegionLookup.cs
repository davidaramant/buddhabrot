using System.Drawing;

namespace Buddhabrot.Core.Boundary;

public sealed class RegionLookup
{
    public IReadOnlyList<uint> Nodes { get; }

    public int Height { get; }
    public int NodeCount => Nodes.Count;

    public static readonly RegionLookup Empty = new();

    private RegionLookup()
    {
        Nodes = new[] {QuadNode2.UnknownLeaf};
        Height = 1;
    }

    public RegionLookup(IReadOnlyList<uint> nodes, int height)
    {
        Nodes = nodes;
        Height = height;
    }

    public IReadOnlyList<(Rectangle Area, RegionType Type)> GetVisibleAreas(
        SquareBoundary bounds,
        IEnumerable<Rectangle> searchAreas)
    {
        var visibleAreas = new List<(Rectangle, RegionType)>();

        foreach (var searchArea in searchAreas)
        {
            var toCheck = new Queue<(SquareBoundary, uint)>();
            toCheck.Enqueue((bounds, Nodes.Last()));

            while (toCheck.Any())
            {
                var (boundary, currentQuad) = toCheck.Dequeue();

                if (currentQuad.GetRegionType() == RegionType.Unknown)
                    continue;

                var intersection = boundary.IntersectWith(searchArea);
                if (intersection == Rectangle.Empty)
                    continue;

                if (currentQuad.GetNodeType() == NodeType.Leaf || boundary.IsPoint)
                {
                    visibleAreas.Add((intersection, currentQuad.GetRegionType()));
                }
                else if (currentQuad.GetNodeType() == NodeType.LeafQuad)
                {
                    toCheck.Enqueue((boundary.LL, QuadNode2.MakeLeaf(currentQuad.GetLL())));
                    toCheck.Enqueue((boundary.LR, QuadNode2.MakeLeaf(currentQuad.GetLR())));
                    toCheck.Enqueue((boundary.UL, QuadNode2.MakeLeaf(currentQuad.GetUL())));
                    toCheck.Enqueue((boundary.UR, QuadNode2.MakeLeaf(currentQuad.GetUR())));
                }
                else
                {
                    toCheck.Enqueue((boundary.LL, Nodes[currentQuad.GetChildIndex(Quadrant.LL)]));
                    toCheck.Enqueue((boundary.LR, Nodes[currentQuad.GetChildIndex(Quadrant.LR)]));
                    toCheck.Enqueue((boundary.UL, Nodes[currentQuad.GetChildIndex(Quadrant.UL)]));
                    toCheck.Enqueue((boundary.UR, Nodes[currentQuad.GetChildIndex(Quadrant.UR)]));
                }
            }

            // Check the mirrored values to build the bottom of the set
            toCheck.Enqueue((bounds, Nodes.Last()));

            while (toCheck.Any())
            {
                var (boundary, currentQuad) = toCheck.Dequeue();

                if (currentQuad.GetRegionType() == RegionType.Unknown)
                    continue;

                var intersection = boundary.IntersectWith(searchArea);
                if (intersection == Rectangle.Empty)
                    continue;

                if (currentQuad.GetNodeType() == NodeType.Leaf || boundary.IsPoint)
                {
                    visibleAreas.Add((intersection, currentQuad.GetRegionType()));
                }
                else if (currentQuad.GetNodeType() == NodeType.LeafQuad)
                {
                    toCheck.Enqueue((boundary.UL, QuadNode2.MakeLeaf(currentQuad.GetLL())));
                    toCheck.Enqueue((boundary.UR, QuadNode2.MakeLeaf(currentQuad.GetLR())));
                    toCheck.Enqueue((boundary.LL, QuadNode2.MakeLeaf(currentQuad.GetUL())));
                    toCheck.Enqueue((boundary.LR, QuadNode2.MakeLeaf(currentQuad.GetUR())));
                }
                else
                {
                    toCheck.Enqueue((boundary.UL, Nodes[currentQuad.GetChildIndex(Quadrant.LL)]));
                    toCheck.Enqueue((boundary.UR, Nodes[currentQuad.GetChildIndex(Quadrant.LR)]));
                    toCheck.Enqueue((boundary.LL, Nodes[currentQuad.GetChildIndex(Quadrant.UL)]));
                    toCheck.Enqueue((boundary.LR, Nodes[currentQuad.GetChildIndex(Quadrant.UR)]));
                }
            }
        }

        return visibleAreas;
    }
}