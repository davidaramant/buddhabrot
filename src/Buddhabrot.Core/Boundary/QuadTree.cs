namespace Buddhabrot.Core.Boundary;

public sealed class QuadTree
{
    private QuadDimensions _dimensions = new(X: 0, Y: 0, Height: 3);
    private QuadNode _root = QuadNode.MakeBranch(RegionType.Unknown, 0);
    private readonly List<QuadNode> _nodes;

    public int Height => _dimensions.Height;
    public IReadOnlyList<QuadNode> Nodes => _nodes;

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
        if (maxDim < _dimensions.SideLength)
        {
            var node = _root;
            var nodeIndex = 0;
            var dimensions = _dimensions;
            bool shouldContinue = true;

            while (shouldContinue)
            {
                var quadrant = dimensions.DetermineQuadrant(id);

                switch (node.NodeType)
                {
                    case NodeType.Branch:
                        nodeIndex = node.GetChildIndex(quadrant); 
                        node = _nodes[nodeIndex];
                        dimensions = dimensions.GetQuadrant(quadrant);
                        break;
                    
                    case NodeType.Leaf:
                        // TODO: if height == 2, convert to LeafQuad
                        // If not, convert to branch and keep descending
                        break;
                    
                    case NodeType.LeafQuad:
                        _nodes[nodeIndex] = node.WithQuadrant(quadrant, type);
                        shouldContinue = false;
                        break;
                }
            }
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

    public QuadTree Normalize()
    {
        throw new NotImplementedException();
    }

    public IReadOnlyList<RegionId> GetBoundaryRegions()
    {
        throw new NotImplementedException();
    }
}