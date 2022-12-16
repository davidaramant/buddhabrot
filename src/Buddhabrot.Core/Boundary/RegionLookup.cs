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
                    toCheck.Enqueue((boundary.SW, QuadNode.MakeLeaf(currentQuad.SW)));
                    toCheck.Enqueue((boundary.SE, QuadNode.MakeLeaf(currentQuad.SE)));
                    toCheck.Enqueue((boundary.NW, QuadNode.MakeLeaf(currentQuad.NW)));
                    toCheck.Enqueue((boundary.NE, QuadNode.MakeLeaf(currentQuad.NE)));
                }
                else
                {
                    toCheck.Enqueue((boundary.SW, Nodes[currentQuad.GetChildIndex(Quadrant.SW)]));
                    toCheck.Enqueue((boundary.SE, Nodes[currentQuad.GetChildIndex(Quadrant.SE)]));
                    toCheck.Enqueue((boundary.NW, Nodes[currentQuad.GetChildIndex(Quadrant.NW)]));
                    toCheck.Enqueue((boundary.NE, Nodes[currentQuad.GetChildIndex(Quadrant.NE)]));
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
                    toCheck.Enqueue((boundary.NW, QuadNode.MakeLeaf(currentQuad.SW)));
                    toCheck.Enqueue((boundary.NE, QuadNode.MakeLeaf(currentQuad.SE)));
                    toCheck.Enqueue((boundary.SW, QuadNode.MakeLeaf(currentQuad.NW)));
                    toCheck.Enqueue((boundary.SE, QuadNode.MakeLeaf(currentQuad.NE)));
                }
                else
                {
                    toCheck.Enqueue((boundary.NW, Nodes[currentQuad.GetChildIndex(Quadrant.SW)]));
                    toCheck.Enqueue((boundary.NE, Nodes[currentQuad.GetChildIndex(Quadrant.SE)]));
                    toCheck.Enqueue((boundary.SW, Nodes[currentQuad.GetChildIndex(Quadrant.NW)]));
                    toCheck.Enqueue((boundary.SE, Nodes[currentQuad.GetChildIndex(Quadrant.NE)]));
                }
            }
        }

        return visibleAreas;
    }
}