using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Buddhabrot.IterationKernels
{
    interface IKernel : IDisposable
    {
        IPointBatch GetBatch();
    }
}
