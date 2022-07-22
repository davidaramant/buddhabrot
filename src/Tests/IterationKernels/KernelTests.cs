using NUnit.Framework;

namespace Tests.IterationKernels;

[TestFixture]
public sealed class KernelTests
{
    //[TestCase(-1.0d, 0.0d)]
    //[TestCase(-0.108938062714878, 0.894186937285121)]
    //public void VectorAndScalarKernelsShouldIterateToSameValue(double real, double imag)
    //{
    //    var c = new Complex(real, imag);

    //    var scalarIterations = IterateWithKernel(new ScalarKernel(), c, 2000);
    //    var vectorIterations = IterateWithKernel(new VectorKernel(), c, 2000);

    //    Assert.That(vectorIterations, Is.EqualTo(scalarIterations), "Vector iteration count was not equal to scalar version.");
    //}

    //[TestCase(-1.0d, 0.0d)]
    //[TestCase(-0.108938062714878, 0.894186937285121)]
    //public void OpenCLAndScalarKernelsShouldIterateToSameValue(double real, double imag)
    //{
    //    var kernelBuilder = new KernelBuilder();
    //    var c = new Complex(real, imag);

    //    var scalarIterations = IterateWithKernel(new ScalarKernel(), c, 2000);
    //    var openCLIterations = IterateWithKernel(kernelBuilder.BuildOpenCL(), c, 2000);

    //    Assert.That(openCLIterations, Is.EqualTo(scalarIterations), "OpenCL iteration count was not equal to scalar version.");
    //}

    //private static long IterateWithKernel(IKernel kernel, Complex c, long maxIterations)
    //{
    //    using (kernel)
    //    {
    //        var batch = kernel.GetBatch();
    //        batch.AddPoint(c);

    //        var result = batch.ComputeIterations(new CancellationToken(), maxIterations);

    //        return result.GetIteration(0);
    //    }
    //}
}