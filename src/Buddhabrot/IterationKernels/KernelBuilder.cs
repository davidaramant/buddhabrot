using System;
using NOpenCL;

namespace Buddhabrot.IterationKernels
{
    public static class KernelBuilder
    {
        public static IKernel Build(KernelType type)
        {
            switch (type)
            {
                case KernelType.ScalarFloat:
                    return new ScalarFloatKernel();
                case KernelType.ScalarDouble:
                    return new ScalarDoubleKernel();
                default:
                    throw new ArgumentException($"Unsupported kernel type: {type}");
            }
        }


        //public IKernel BuildOpenCL()
        //{
        //    return null;
        //    //return new OpenCLKernel(Platform.GetPlatforms()[0].GetDevices(DeviceType.Gpu).Single());
        //}
    }
}
