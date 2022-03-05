namespace Buddhabrot.IterationKernels;

public static class KernelBuilder
{
    public static IScalarKernel BuildScalarKernel(ComputationType type) =>
        type switch
        {
            ComputationType.ScalarFloat => new ScalarFloatKernel(),
            ComputationType.ScalarDouble => new ScalarDoubleKernel(),
            _ => throw new ArgumentException($"{type} is not a scalar type.")
        };
}