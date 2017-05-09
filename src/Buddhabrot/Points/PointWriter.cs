using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Buddhabrot.Extensions;
using log4net;

namespace Buddhabrot.Points
{
    sealed class PointWriter
    {
        private static readonly ILog Log = LogManager.GetLogger(nameof(PointWriter));

        private readonly object _fileLock = new object();
        private readonly string _filePath;

        public PointWriter(string filePath)
        {
            _filePath = filePath;
        }

        public void Save(Complex point)
        {
            lock (_fileLock)
            {
                using (var stream = File.Open(_filePath, FileMode.Append))
                using (var writer = new BinaryWriter(stream))
                {
                    writer.WriteComplex(point);
                }
            }
        }

        public void Save(IEnumerable<Complex> points)
        {
            lock (_fileLock)
            {
                using (var stream = File.Open(_filePath, FileMode.Append))
                using (var writer = new BinaryWriter(stream))
                {
                    foreach (var point in points)
                    {
                        writer.WriteComplex(point);
                    }
                }
            }
        }
    }
}
