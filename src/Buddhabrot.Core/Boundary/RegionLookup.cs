using System.Drawing;
using Buddhabrot.Core.Boundary.QuadTrees;

namespace Buddhabrot.Core.Boundary;

public sealed class RegionLookup
{
    public IReadOnlyList<QuadNode> Nodes { get; }

    public int Height { get; }
    public int NodeCount => Nodes.Count;

    public static readonly RegionLookup Empty = new();

    private RegionLookup()
    {
        Nodes = new[] {QuadNode.UnknownLeaf};
        Height = 1;
    }

    public RegionLookup(IReadOnlyList<QuadNode> nodes, int height)
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
            var toCheck = new Queue<(SquareBoundary, QuadNode)>();
            toCheck.Enqueue((bounds, Nodes.Last()));

            while (toCheck.Any())
            {
                var (boundary, currentQuad) = toCheck.Dequeue();

                if (currentQuad.RegionType == RegionType.Unknown)
                    continue;

                var intersection = boundary.IntersectWith(searchArea);
                if (intersection == Rectangle.Empty)
                    continue;

                if (currentQuad.NodeType == NodeType.Leaf || boundary.IsPoint)
                {
                    visibleAreas.Add((intersection, currentQuad.RegionType));
                }
                else if (currentQuad.NodeType == NodeType.LeafQuad)
                {
                    toCheck.Enqueue((boundary.LL, QuadNode.MakeLeaf(currentQuad.LL)));
                    toCheck.Enqueue((boundary.LR, QuadNode.MakeLeaf(currentQuad.LR)));
                    toCheck.Enqueue((boundary.UL, QuadNode.MakeLeaf(currentQuad.UL)));
                    toCheck.Enqueue((boundary.UR, QuadNode.MakeLeaf(currentQuad.UR)));
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

                if (currentQuad.RegionType == RegionType.Unknown)
                    continue;

                var intersection = boundary.IntersectWith(searchArea);
                if (intersection == Rectangle.Empty)
                    continue;

                if (currentQuad.NodeType == NodeType.Leaf || boundary.IsPoint)
                {
                    visibleAreas.Add((intersection, currentQuad.RegionType));
                }
                else if (currentQuad.NodeType == NodeType.LeafQuad)
                {
                    toCheck.Enqueue((boundary.UL, QuadNode.MakeLeaf(currentQuad.LL)));
                    toCheck.Enqueue((boundary.UR, QuadNode.MakeLeaf(currentQuad.LR)));
                    toCheck.Enqueue((boundary.LL, QuadNode.MakeLeaf(currentQuad.UL)));
                    toCheck.Enqueue((boundary.LR, QuadNode.MakeLeaf(currentQuad.UR)));
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