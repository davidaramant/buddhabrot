using System;
using System.Collections.Generic;
using System.Linq;
using Buddhabrot.Edges;

namespace Buddhabrot.Points
{
    public sealed class WorkSequence
    {
        private readonly IEnumerable<PointPair> _pairSequence;
        private readonly Stack<PointPair> _addedWorkBuffer = new Stack<PointPair>();

        public WorkSequence(IEnumerable<PointPair> pairSequence)
        {
            _pairSequence = pairSequence;
        }

        public void AddAdditional(IEnumerable<PointPair> pairs)
        {
            foreach (var pair in pairs)
            {
                _addedWorkBuffer.Push(pair);
            }
        }

        public IEnumerable<PointPair> Take(int batchSize)
        {
            int fromBuffer = Math.Min(batchSize, _addedWorkBuffer.Count);
            int remaining = batchSize - _addedWorkBuffer.Count;
            for(int i = 0; i < fromBuffer; i++)
            {
                yield return _addedWorkBuffer.Pop();
            }

            if (remaining > 0)
            {
                foreach (var pair in _pairSequence.Take(batchSize))
                {
                    yield return pair;
                }
            }
        }
    }
}
