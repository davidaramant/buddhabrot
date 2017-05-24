using System;
using System.IO;

namespace Buddhabrot.IterationKernels
{
    static class OpenCLKernelSource
    {
        private const string FloatSourceName = "Buddhabrot.IterationKernels.iterate_points_f.cl";
        private static readonly Lazy<string> FloatSourceLoader = new Lazy<string>(() => ReadResource(FloatSourceName));

        private const string DoubleSourceName = "Buddhabrot.IterationKernels.iterate_points_d.cl";
        private static readonly Lazy<string> DoubleSourceLoader = new Lazy<string>(() => ReadResource(DoubleSourceName));

        private static string ReadResource(string name)
        {
            using (var stream = typeof(OpenCLKernelSource).Assembly.GetManifestResourceStream(name))
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

        public static string ReadFloatVersion() => FloatSourceLoader.Value;
        public static string ReadDoubleVersion() => DoubleSourceLoader.Value;
    }
}
