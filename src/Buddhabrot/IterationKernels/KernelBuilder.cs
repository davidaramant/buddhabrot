namespace Buddhabrot.IterationKernels;

public static class KernelBuilder
{
    public static IScalarKernel BuildScalarKernel(ComputationType type)
    {
        switch (type)
        {
            case ComputationType.ScalarFloat:
                return new ScalarFloatKernel();
            case ComputationType.ScalarDouble:
                return new ScalarDoubleKernel();
            default:
                throw new ArgumentException($"{type} is not a scalar type.");
        }
    }
}