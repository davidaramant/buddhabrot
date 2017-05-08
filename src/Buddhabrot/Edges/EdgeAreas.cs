using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using Buddhabrot.Core;
using Buddhabrot.Extensions;
using log4net;

namespace Buddhabrot.Edges
{
    public sealed class EdgeAreas : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(nameof(EdgeAreas));

        private const string HeaderText = "Edge Areas V1.00";
        // A sixteen byte magic header value
        private static readonly byte[] MagicBytes = Encoding.ASCII.GetBytes(HeaderText);

        public Size GridResolution { get; }
        public ComplexArea ViewPort { get; }
        public int AreaCount { get; }

        private readonly Stream _areaStream;
        private readonly long _areasPosition;

        public static EdgeAreas Load(string filePath)
        {
            FileStream stream = null;

            try
            {
                stream = File.OpenRead(filePath);

                using (var reader = new BinaryReader(stream, Encoding.ASCII, leaveOpen: true))
                {
                    var chars = reader.ReadChars(HeaderText.Length);
                    var readHeader = new string(chars);
                    if (readHeader != HeaderText)
                        throw new InvalidOperationException($"Edges file was corrupt: {filePath}");

                    var size = reader.ReadSize();
                    var viewPort = reader.ReadComplexArea();
                    var count = reader.ReadInt32();
                    Log.Info($"Loaded edges with resolution ({size.Width:N0}x{size.Height:N0}), view port {viewPort}, and {count:N0} edge areas.");
                    return new EdgeAreas(size, viewPort, count, stream);
                }
            }
            catch (Exception)
            {
                stream?.Dispose();
                throw;
            }
        }

        private EdgeAreas(
            Size gridResolution,
            ComplexArea viewPort,
            int areaCount,
            Stream areaStream)
        {
            GridResolution = gridResolution;
            ViewPort = viewPort;
            AreaCount = areaCount;
            _areaStream = areaStream;
            _areasPosition = areaStream.Position;
        }

        public IEnumerable<Point> GetAreaLocations() => GetAreas().Select(ea => ea.GridLocation);

        public IEnumerable<EdgeArea> GetAreas()
        {
            _areaStream.Position = _areasPosition;
            using (var reader = new BinaryReader(_areaStream, Encoding.Default, leaveOpen: true))
            {
                while (_areaStream.Position < _areaStream.Length)
                {
                    yield return reader.ReadEdgeArea();
                }
            }
        }

        public void Dispose()
        {
            _areaStream.Dispose();
        }

        public static void Write(
            Size gridResolution,
            ComplexArea viewPort,
            IEnumerable<EdgeArea> areas,
            string filePath)
        {
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                stream.Write(MagicBytes, 0, MagicBytes.Length);

                using (var writer = new BinaryWriter(stream))
                {
                    writer.WriteSize(gridResolution);
                    writer.WriteComplexArea(viewPort);

                    long countPosition = stream.Position;
                    stream.Position += sizeof(int);

                    int count = 0;
                    foreach (var area in areas)
                    {
                        count++;
                        writer.WriteEdgeAreaLegacy(area);
                    }
                    Log.Info($"Wrote {count:N0} edge areas.");

                    stream.Position = countPosition;
                    writer.Write(count);
                }
            }
        }
    }
}
