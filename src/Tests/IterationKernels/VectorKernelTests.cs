using System.Numerics;
using System.Threading;
using Buddhabrot.Core;
using Buddhabrot.IterationKernels;
using NUnit.Framework;

namespace Tests.IterationKernels
{
    [TestFixture]
    public sealed class VectorKernelTests
    {
        [TestCase(-1.0d, 0.0d)]
        [TestCase(-0.108938062714878, 0.894186937285121)]
        public void ShouldIterateToSameValue(double real, double imag)
        {
            var range = new IntRange(0, 2000);
            var c = new Complex(real, imag);

            var scalarIterations = IterateWithKernel(new ScalarKernel(range), c);
            var vectorIterations = IterateWithKernel(new VectorKernel(range), c);

            Assert.That(vectorIterations, Is.EqualTo(scalarIterations), "Vector iteration count was not equal to scalar version.");
        }

        private static long IterateWithKernel(IKernel kernel, Complex c)
        {
            using (kernel)
            {
                var batch = kernel.GetBatch();
                batch.AddPoint(c);

                var result = batch.ComputeIterations(new CancellationToken());

                return result.GetIteration(0);
            }
        }
    }
}
