namespace Buddhabrot.Core.Utilities;

/// <summary>
/// Keeps track of remaining work.
/// </summary>
public sealed class WorkRemaining<T> : IDisposable
{
    private readonly IEnumerator<T> _enumerator;

    // Stacks are fast since they are array based
    // The order of the work is irrelevant, so the LIFO behavior doesn't matter
    private readonly Stack<T> _addedWorkBuffer = new();

    public WorkRemaining(IEnumerable<T> sequence) => _enumerator = sequence.GetEnumerator();

    /// <summary>
    /// Adds additional work.  It will be consumed in a LIFO order.
    /// </summary>
    /// <param name="work">The work.</param>
    public void AddAdditional(IEnumerable<T> work)
    {
        foreach (var item in work)
        {
            _addedWorkBuffer.Push(item);
        }
    }

    public IEnumerable<T> Take(int batchSize)
    {
        int fromBuffer = Math.Min(batchSize, _addedWorkBuffer.Count);
        int fromSequence = batchSize - _addedWorkBuffer.Count;
        for (int i = 0; i < fromBuffer; i++)
        {
            yield return _addedWorkBuffer.Pop();
        }

        for (int i = 0; i < fromSequence; i++)
        {
            if (_enumerator.MoveNext())
            {
                yield return _enumerator.Current;
            }
            else
            {
                yield break;
            }
        }
    }

    public void Dispose() => _enumerator.Dispose();
}
