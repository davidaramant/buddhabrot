namespace Buddhabrot.IterationKernels;

public interface IKernel : IDisposable
{
    ComputationType Type { get; }
    IPointBatch GetBatch();
}