using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Buddhabrot.Core;

namespace Buddhabrot.IterationKernels
{
    sealed class NoResults : IPointBatchResults
    {
        public static readonly IPointBatchResults Instance = new NoResults();

        public int Count => 0;
        public FComplex GetPoint(int index)
        {
            throw new NotImplementedException();
        }

        public int GetIteration(int index)
        {
            throw new NotImplementedException();
        }

        private NoResults()
        {
            
        }
    }
}
