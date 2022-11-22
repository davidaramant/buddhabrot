namespace Buddhabrot.Core.Boundary;

public sealed class QuadTree
{
    private QuadNode _root = QuadNode.MakeBranch(RegionType.Unknown, 0);
    private readonly List<QuadNode> _nodes;

    public int Height { get; private set; } = 3;

    public int Width => 1 << (Height - 1);

    public QuadTree(int capacity = 0)
    {
        _nodes = new(capacity)
        {
            QuadNode.EmptyQuadLeaf,
            QuadNode.EmptyQuadLeaf,
            QuadNode.EmptyQuadLeaf,
            QuadNode.EmptyQuadLeaf,
        };
    }

    public void Add(RegionId id, RegionType type)
    {
        int maxDim = Math.Max(id.X, id.Y);
        if (maxDim < Width)
        {
            // Mutate existing tree
        }
        else
        {
            // Expand tree
        }
    }

    public bool Contains(RegionId id)
    {
        throw new NotImplementedException();
    }

    public QuadTree Compress()
    {
        throw new NotImplementedException();
    }

    public IReadOnlyList<RegionId> GetBoundaryRegions()
    {
        throw new NotImplementedException();
    }
}