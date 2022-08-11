namespace Buddhabrot.Core.Boundary.RegionQuadTree;

sealed class TransformCache<TInput, TOutput>
    where TInput : notnull
    where TOutput : notnull
{
    private readonly Dictionary<TInput, TOutput> _dict = new();
    private readonly Func<TInput, TOutput> _f;

    public TOutput Transform(TInput a)
    {
        if (!_dict.TryGetValue(a, out var r))
        {
            r = _f(a);
            _dict.Add(a, r);
        }

        return r;
    }

    public TransformCache(Func<TInput, TOutput> f) => _f = f;

    public int Count => _dict.Count;
}