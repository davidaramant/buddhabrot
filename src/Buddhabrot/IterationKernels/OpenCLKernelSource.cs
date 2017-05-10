using System;
using System.IO;

namespace Buddhabrot.IterationKernels
{
    static class OpenCLKernelSource
    {
        private const string SourceName = "Buddhabrot.IterationKernels.iterate_points.cl";
        private static readonly Lazy<string> SourceLoader = new Lazy<string>(() =>
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
        });


        public static string Read() => SourceLoader.Value;
    }
}
