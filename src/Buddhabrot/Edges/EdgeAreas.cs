using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using Buddhabrot.Core;
using log4net;

namespace Buddhabrot.Edges
{
    public sealed class EdgeAreas
    {
        private static readonly ILog Log = LogManager.GetLogger(nameof(EdgeLocator));

        private const string HeaderText = "Float Areas V1.0";
        // A sixteen byte magic header value
        private static readonly byte[] MagicBytes = Encoding.ASCII.GetBytes(HeaderText);

        public Size GridResolution { get; }
        public ComplexArea ViewPort { get; }
        private readonly List<EdgeArea> _areas;
        public int AreaCount => _areas.Count;

        public IEnumerable<Point> GetAreaLocations()
        {
            return _areas.Select(a => a.GridLocation);
        }

        private EdgeAreas(
            Size gridResolution,
            ComplexArea viewPort,
            List<EdgeArea> areas)
        {
            GridResolution = gridResolution;
            ViewPort = viewPort;
            _areas = areas;
        }


        public static void Write(
            string filePath,
            Size gridResolution,
            ComplexArea viewPort,
            IEnumerable<EdgeArea> areas)
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

                void WriteEdgeArea(EdgeArea edgeArea)
                {
                    WriteArea(edgeArea.Area);
                    writer.Write(edgeArea.GridLocation.X);
                    writer.Write(edgeArea.GridLocation.Y);
                    writer.Write((int)edgeArea.EncodingDirection);
                    writer.Write(edgeArea.Length);
                }

                writer.Write(gridResolution.Width);
                writer.Write(gridResolution.Height);
                WriteArea(viewPort);

                int count = 0;
                foreach (var area in areas)
                {
                    count++;
                    WriteEdgeArea(area);
                }
                Log.Info($"Wrote {count} edge areas.");
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
                    Range ReadRange() => new Range(reader.ReadSingle(), reader.ReadSingle());
                    ComplexArea ReadArea() => new ComplexArea(ReadRange(), ReadRange());
                    EdgeArea ReadEdgeArea()
                    {
                        var area = ReadArea();
                        var x = reader.ReadInt32();
                        var y = reader.ReadInt32();
                        var direction = (EncodingDirection)reader.ReadInt32();
                        var length = reader.ReadInt32();

                        return new EdgeArea(area, new Point(x, y), direction, length);
                    }

                    var width = reader.ReadInt32();
                    var height = reader.ReadInt32();

                    var viewPort = ReadArea();

                    var edgeAreas = new List<EdgeArea>();

                    while (stream.Position < stream.Length)
                    {
                        edgeAreas.Add(ReadEdgeArea());
                    }

                    Log.Info($"Loaded edges with resolution ({width:N0}x{height:N0}), view port {viewPort}, and {edgeAreas.Count} edge areas.");

                    return new EdgeAreas(
                        new Size(width, height),
                        viewPort,
                        edgeAreas);
                }
            }
        }
    }
}
