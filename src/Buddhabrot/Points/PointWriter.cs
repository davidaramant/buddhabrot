using System.Collections.Generic;
using System.IO;
using Buddhabrot.Core;
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

        public void Save(FComplex point)
        {
            lock (_fileLock)
            {
                using (var stream = File.Open(_filePath, FileMode.Append))
                using (var writer = new BinaryWriter(stream))
                {
                    writer.WriteComplex(point);
                    Log.Info("Wrote a point!");
                }
            }
        }

        public void Save(IEnumerable<FComplex> points)
        {
            lock (_fileLock)
            {
                using (var stream = File.Open(_filePath, FileMode.Append))
                using (var writer = new BinaryWriter(stream))
                {
                    int count = 0;
                    foreach (var point in points)
                    {
                        writer.WriteComplex(point);
                        count++;
                    }
                    Log.Info($"Wrote {count} points!");
                }
            }
        }
    }
}
