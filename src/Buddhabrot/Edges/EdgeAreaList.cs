using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Buddhabrot.Core;

namespace Buddhabrot.Edges
{
    static class EdgeAreaList
    {
        private const string HeaderText = "Float Areas V1.0";
        // A sixteen byte magic header value
        private static readonly byte[] MagicBytes = Encoding.ASCII.GetBytes(HeaderText);

        public static void SaveAreas(string filePath, IEnumerable<ComplexArea> areas)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            File.WriteAllBytes(filePath, MagicBytes);

            using (var stream = new FileStream(filePath, FileMode.Append))
            using (var writer = new BinaryWriter(stream))
            {
                foreach (var area in areas)
                {
                    void WriteRange(Range range)
                    {
                        writer.Write(range.InclusiveMin);
                        writer.Write(range.ExclusiveMax);
                    }

                    WriteRange(area.RealRange);
                    WriteRange(area.ImagRange);
                }
            }
        }

        public static IEnumerable<ComplexArea> ReadAreas(string filePath)
        {
            using (var stream = File.OpenRead(filePath))
            {
                var header = new byte[MagicBytes.Length];
                var bytesRead = stream.Read(header, 0, header.Length);
                if (bytesRead != header.Length || Encoding.ASCII.GetString(header) != HeaderText)
                    throw new InvalidOperationException($"Edges file was corrupt: {filePath}");

                using (var reader = new BinaryReader(stream))
                {
                    Range ReadRange()
                    {
                        return new Range(reader.ReadSingle(), reader.ReadSingle());
                    }

                    yield return new ComplexArea(
                        realRange: ReadRange(),
                        imagRange: ReadRange());
                }
            }
        }
    }
}
