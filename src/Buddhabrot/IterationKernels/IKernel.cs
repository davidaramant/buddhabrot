using System;

namespace Buddhabrot.IterationKernels
{
    public interface IKernel : IDisposable
    {
        IPointBatch GetBatch();
    }
}
