using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using Buddhabrot.Core;
using Buddhabrot.Extensions;
using log4net;

namespace Buddhabrot.EdgeSpans
{
    public sealed class EdgeSpanStream : IEnumerable<(Point Location, Direction Direction, EdgeSpan Span)>, IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(nameof(EdgeSpanStream));
        private const string HeaderText = "Edge Spans V2.00";

        // Since the direction is only a byte, we'll pack it into the top of the Y position
        // This saves a byte of disk space and, uh, has better alignment?
        // Yes, it's pointless, but kind of fun!
        private const int DirectionOffset = 8 * (sizeof(int) - sizeof(byte));

        public ViewPort ViewPort { get; }
        public int Count { get; }
        private readonly Stream _spanStream;
        private readonly long _spansPosition;

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

                    var viewPort = reader.ReadViewPort();
                    var count = reader.ReadInt32();
                    Log.Info($"Loaded edge spans with resolution ({viewPort.Resolution.Width:N0}x{viewPort.Resolution.Height:N0}), " +
                             $"area {viewPort.Area}, and {count:N0} edge spans.");
                    return new EdgeSpanStream(viewPort, count, stream);
                }
            }
            catch (Exception)
            {
                stream?.Dispose();
                throw;
            }
        }

        private EdgeSpanStream(
            ViewPort viewPort,
            int count,
            Stream spanStream)
        {
            ViewPort = viewPort;
            Count = count;
            _spanStream = spanStream;
            _spansPosition = spanStream.Position;
        }

        public IEnumerator<(Point Location, Direction Direction, EdgeSpan Span)> GetEnumerator()
        {
            _spanStream.Position = _spansPosition;
            using (var reader = new BinaryReader(_spanStream, Encoding.ASCII, leaveOpen: true))
            {
                while (_spanStream.Position < _spanStream.Length)
                {
                    var x = reader.ReadInt32();
                    var packedY = reader.ReadInt32();

                    var y = packedY & 0x00FFFFFF;
                    var direction = (Direction)(packedY >> DirectionOffset & 0xFF);

                    var location = new Point(x, y);
                    yield return (location,
                        direction,
                        new EdgeSpan(
                            inSet: ViewPort.GetComplex(location),
                            notInSet: ViewPort.GetComplex(location.OffsetIn(direction))));
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Dispose() => _spanStream.Dispose();

        public static void Write(
            string filePath,
            ViewPort viewPort,
            IEnumerable<LogicalEdgeSpan> spans)
        {
            if (viewPort.Resolution.Height > (1 << DirectionOffset))
            {
                throw new ArgumentOutOfRangeException(nameof(viewPort.Resolution), "That resolution is too damn big.");
            }

            using (var stream = new FileStream(filePath, FileMode.Create))
            using (var writer = new BinaryWriter(stream, Encoding.ASCII))
            {
                writer.Write(HeaderText);
                writer.WriteViewPort(viewPort);

                long countPosition = stream.Position;
                stream.Position += sizeof(int);

                int count = 0;

                foreach (var span in spans)
                {
                    count++;

                    writer.Write(span.X);
                    writer.Write(span.Y | ((int)span.ToOutside << DirectionOffset));
                }

                Log.Info($"Wrote {count:N0} edge spans.");

                stream.Position = countPosition;
                writer.Write(count);
            }
        }
    }
}
