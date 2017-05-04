using System;
using System.IO;

namespace Buddhabrot.IterationKernels
{
    static class OpenCLKernelSource
    {
        private const string SourceName = "Buddhabrot.IterationKernels.iterate_points.cl";

        public static string Read()
        {
            using (var stream = typeof(OpenCLKernelSource).Assembly.GetManifestResourceStream(SourceName))
            {
                if (stream == null)
                {
                    throw new InvalidOperationException("Could not load the OpenCL kernel source.");
                }

                using (var streamReader = new StreamReader(stream))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }
    }
}
