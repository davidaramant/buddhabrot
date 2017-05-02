using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using Buddhabrot.Core;

namespace Buddhabrot.Edges
{
    public sealed class EdgeAreas
    {
        private const string HeaderText = "Float Areas V1.0";
        // A sixteen byte magic header value
        private static readonly byte[] MagicBytes = Encoding.ASCII.GetBytes(HeaderText);

        public Size GridResolution { get; }
        public ComplexArea ViewPort { get; }
        private readonly List<ComplexArea> _areas;
        public int AreaCount => _areas.Count;

        private EdgeAreas(
            Size gridResolution,
            ComplexArea viewPort,
            List<ComplexArea> areas)
        {
            GridResolution = gridResolution;
            ViewPort = viewPort;
            _areas = areas;
        }


        public static void Write(
            string filePath,
            Size gridResolution,
            ComplexArea viewPort,
            IEnumerable<ComplexArea> areas)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            File.WriteAllBytes(filePath, MagicBytes);

            using (var stream = new FileStream(filePath, FileMode.Append))
            using (var writer = new BinaryWriter(stream))
            {
                void WriteRange(Range range)
                {
                    writer.Write(range.InclusiveMin);
                    writer.Write(range.ExclusiveMax);
                }

                void WriteArea(ComplexArea area)
                {
                    WriteRange(area.RealRange);
                    WriteRange(area.ImagRange);
                }

                writer.Write(gridResolution.Width);
                writer.Write(gridResolution.Height);
                WriteArea(viewPort);

                foreach (var area in areas)
                {
                    WriteArea(area);
                }
            }
        }

        public static EdgeAreas Load(string filePath)
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

                    ComplexArea ReadArea()
                    {
                        return new ComplexArea(ReadRange(), ReadRange());
                    }

                    var width = reader.ReadInt32();
                    var height = reader.ReadInt32();

                    var viewPort = ReadArea();

                    var edgeAreas = new List<ComplexArea>();

                    while (stream.Position < stream.Length)
                    {
                        edgeAreas.Add(ReadArea());
                    }

                    return new EdgeAreas(
                        new Size(width, height),
                        viewPort,
                        edgeAreas);
                }
            }
        }
    }
}
