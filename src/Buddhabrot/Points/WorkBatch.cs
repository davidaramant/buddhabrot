using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Buddhabrot.Core;
using Buddhabrot.Edges;
using Buddhabrot.IterationKernels;

namespace Buddhabrot.Points
{
    public sealed class WorkBatch
    {
        private readonly Func<IPointBatch> _batchGetter;
        private readonly List<PointPair> _pairs = new List<PointPair>();
        private IPointBatch _batch;
        private IPointBatchResults _results;

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

        public IEnumerable<(PointPair pair, FComplex middle, int iterations)> GetResults()
        {
            for (int i = 0; i < _results.Count; i++)
            {
                yield return (_pairs[i], _results.GetPoint(i), _results.GetIteration(i));
            }
        }
    }
}
