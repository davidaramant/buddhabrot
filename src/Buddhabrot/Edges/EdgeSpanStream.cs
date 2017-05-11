using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using Buddhabrot.Core;
using Buddhabrot.Extensions;
using log4net;

namespace Buddhabrot.Edges
{
    public sealed class EdgeSpanStream : IEnumerable<EdgeSpan>, IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(nameof(EdgeSpanStream));
        private const string HeaderText = "Edge Spans V1.00";

        public Size PointResolution { get; }
        public ComplexArea ViewPort { get; }
        public int Count { get; }
        private readonly Stream _spanStream;
        private readonly long _spansPosition;
        private readonly PositionCalculator _positionCalculator;

        public static EdgeSpanStream Load(string filePath)
        {
            FileStream stream = null;

            try
            {
                stream = File.OpenRead(filePath);

                using (var reader = new BinaryReader(stream, Encoding.ASCII, leaveOpen: true))
                {
                    var readHeader = reader.ReadString(); ;
                    if (readHeader != HeaderText)
                        throw new InvalidOperationException($"Unsupported edge span file format: {filePath}");

                    var size = reader.ReadSize();
                    var viewPort = reader.ReadComplexArea();
                    var count = reader.ReadInt32();
                    Log.Info($"Loaded edge spans with resolution ({size.Width:N0}x{size.Height:N0}), " +
                             $"view port {viewPort}, and {count:N0} edge spans.");
                    return new EdgeSpanStream(size, viewPort, count, stream);
                }
            }
            catch (Exception)
            {
                stream?.Dispose();
                throw;
            }
        }

        private EdgeSpanStream(
            Size pointResolution,
            ComplexArea viewPort,
            int count,
            Stream spanStream)
        {
            PointResolution = pointResolution;
            ViewPort = viewPort;
            Count = count;
            _spanStream = spanStream;
            _spansPosition = spanStream.Position;
            _positionCalculator = new PositionCalculator(PointResolution, viewPort);
        }

        public IEnumerator<EdgeSpan> GetEnumerator()
        {
            _spanStream.Position = _spansPosition;
            using (var reader = new BinaryReader(_spanStream, Encoding.ASCII, leaveOpen: true))
            {
                while (_spanStream.Position < _spanStream.Length)
                {
                    var x = reader.ReadInt32();
                    var packedY = reader.ReadInt32();

                    var y = packedY & 0x00FFFFFF;
                    var direction = (Direction)(packedY >> 24 & 0xFF);

                    var location = new Point(x, y);
                    yield return new EdgeSpan(
                        inSet: _positionCalculator.GetPoint(location),
                        notInSet: _positionCalculator.GetPoint(location.OffsetIn(direction)));
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Dispose() => _spanStream.Dispose();

        public static void Write(
            string filePath,
            Size pointResolution,
            ComplexArea viewPort,
            IEnumerable<LogicalEdgeSpan> spans)
        {
            if (pointResolution.Height > Math.Pow(2, 32 - sizeof(byte)))
            {
                throw new ArgumentOutOfRangeException(nameof(pointResolution), "That resolution is too damn big.");
            }

            using (var stream = new FileStream(filePath, FileMode.Create))
            using (var writer = new BinaryWriter(stream, Encoding.ASCII))
            {
                writer.Write(HeaderText);
                writer.WriteSize(pointResolution);
                writer.WriteComplexArea(viewPort);

                long countPosition = stream.Position;
                stream.Position += sizeof(int);

                int count = 0;

                foreach (var span in spans)
                {
                    count++;

                    writer.Write(span.X);
                    writer.Write(span.Y | ((int)span.ToOutside << 24));
                }

                Log.Info($"Wrote {count:N0} edge spans.");

                stream.Position = countPosition;
                writer.Write(count);
            }
        }
    }
}
