using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using Buddhabrot.EdgeSpans;
using Buddhabrot.IterationKernels;

namespace Buddhabrot.Points
{
    public sealed class WorkBatch
    {
        private readonly Func<IPointBatch> _batchGetter;
        private readonly List<EdgeSpan> _spans = new List<EdgeSpan>();
        private IPointBatch _batch;
        private IPointBatchResults _results;

        public int Count => _batch?.Count ?? 0;
        public int Capacity => _batch?.Capacity ?? 0;

        public WorkBatch(Func<IPointBatch> batchGetter)
        {
            _batchGetter = batchGetter;
        }

        public void Reset()
        {
            _batch = _batchGetter();
            _spans.Clear();
        }

        public void Add(EdgeSpan span)
        {
            _spans.Add(span);
            _batch.AddPoint(span.GetMidPoint());
        }

        public void Compute(CancellationToken token, long maxIterations)
        {
            _results = _batch.ComputeIterations(token, maxIterations);
        }

        public IEnumerable<(EdgeSpan span, Complex middle, long iterations)> GetResults()
        {
            for (int i = 0; i < _results.Count; i++)
            {
                yield return (_spans[i], _results.GetPoint(i), _results.GetIteration(i));
            }
        }
    }
}
