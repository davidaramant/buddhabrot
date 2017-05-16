using System.Numerics;
using Buddhabrot.Core;

namespace Buddhabrot.IterationKernels
{
    public interface IScalarKernel
    {
        EscapeTime FindEscapeTime(Complex c);
        EscapeTime FindEscapeTime(Complex c, int maxIterations);
    }
}
