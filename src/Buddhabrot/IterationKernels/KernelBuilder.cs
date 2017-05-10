using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NOpenCL;

namespace Buddhabrot.IterationKernels
{
    public sealed class KernelBuilder
    {
        public IKernel BuildOpenCL()
        {
            return new OpenCLKernel(Platform.GetPlatforms()[0].GetDevices(DeviceType.Gpu).Single());
        }
    }
}
