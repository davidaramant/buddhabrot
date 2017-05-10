using System;
using System.Collections.Generic;
using Buddhabrot.Core;
using log4net;

namespace Buddhabrot.Points
{
    /// <summary>
    /// Keeps track of remaining work in an undefined order.
    /// </summary>
    public sealed class WorkRemaining : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(nameof(WorkRemaining));

        private readonly IEnumerator<EdgeSpan> _pairEnumerator;

        // Stacks are fast since they are array based
        // The order of the work is irrelevant, so the LIFO behavior doesn't matter
        private readonly Stack<EdgeSpan> _addedWorkBuffer = new Stack<EdgeSpan>();

        public WorkRemaining(IEnumerable<EdgeSpan> pairSequence)
        {
            _pairEnumerator = pairSequence.GetEnumerator();
        }

        public void AddAdditional(IEnumerable<EdgeSpan> pairs)
        {
            foreach (var pair in pairs)
            {
                _addedWorkBuffer.Push(pair);
            }
        }

        public IEnumerable<EdgeSpan> Take(int batchSize)
        {
            int fromBuffer = Math.Min(batchSize, _addedWorkBuffer.Count);
            int remaining = batchSize - _addedWorkBuffer.Count;
            for(int i = 0; i < fromBuffer; i++)
            {
                yield return _addedWorkBuffer.Pop();
            }

            for (int i = 0; i < remaining; i++)
            {
                if (_pairEnumerator.MoveNext())
                {
                    yield return _pairEnumerator.Current;
                }
                else
                {
                    yield break;
                }

            }
        }

        public void Dispose()
        {
            _pairEnumerator.Dispose();
        }
    }
}
