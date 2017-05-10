using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using Buddhabrot.Core;
using Buddhabrot.IterationKernels;

namespace Buddhabrot.Points
{
    public sealed class WorkBatch
    {
        private readonly Func<IPointBatch> _batchGetter;
        private readonly List<PointPair> _pairs = new List<PointPair>();
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
            _pairs.Clear();
        }

        public void Add(PointPair pair)
        {
            _pairs.Add(pair);
            _batch.AddPoint(pair.GetMidPoint());
        }

        public void Compute(CancellationToken token)
        {
            _results = _batch.ComputeIterations(token);
        }

        public IEnumerable<(PointPair pair, Complex middle, long iterations)> GetResults()
        {
            for (int i = 0; i < _results.Count; i++)
            {
                yield return (_pairs[i], _results.GetPoint(i), _results.GetIteration(i));
            }
        }
    }
}
