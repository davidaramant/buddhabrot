using System;

namespace Buddhabrot.IterationKernels
{
    public interface IKernel : IDisposable
    {
        KernelType Type { get; }
        IPointBatch GetBatch();
    }
}
