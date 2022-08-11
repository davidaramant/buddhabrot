namespace Buddhabrot.Core.Boundary.RegionQuadTree;

sealed class TransformCache<TInput, TOutput>
    where TInput : notnull
    where TOutput : notnull
{
    private readonly Dictionary<TInput, TOutput> _dict = new();
    private readonly Func<TInput, TOutput> _f;

    public int Size => _dict.Count;
    public int NumCachedValuesUsed { get; private set; }
    
    public TransformCache(Func<TInput, TOutput> f) => _f = f;
    
    public TOutput Transform(TInput a)
    {
        if (!_dict.TryGetValue(a, out var r))
        {
            r = _f(a);
            _dict.Add(a, r);
        }
        else
        {
            NumCachedValuesUsed++;
        }

        return r;
    }
}