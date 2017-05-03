using System.Collections.Generic;
using System.IO;
using Buddhabrot.Core;
using Buddhabrot.Extensions;

namespace Buddhabrot.Points
{
    sealed class PointWriter
    {
        private readonly object _fileLock = new object();
        private readonly string _filePath;

        public PointWriter(string filePath)
        {
            _filePath = filePath;
        }

        public void Save(IEnumerable<FComplex> points)
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
