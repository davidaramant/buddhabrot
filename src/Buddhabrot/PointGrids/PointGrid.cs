using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using Buddhabrot.Core;
using Buddhabrot.Extensions;
using log4net;

namespace Buddhabrot.PointGrids
{
    public sealed class PointGrid : IDisposable, IEnumerable<PointRow>
    {
        private static readonly ILog Log = LogManager.GetLogger(nameof(PointGrid));
        //                                 0123456789abcdef
        private const string HeaderText = "Point Grid V1.00";

        public Size PointResolution { get; }
        public ComplexArea ViewPort { get; }

        private readonly Stream _pointStream;
        private readonly long _pointsPosition;

        public static PointGrid Load(string filePath)
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
                    Log.Info($"Loaded point grid with resolution ({size.Width:N0}x{size.Height:N0}), " +
                             $"view port {viewPort}.");
                    return new PointGrid(size, viewPort, stream);
                }
            }
            catch (Exception)
            {
                stream?.Dispose();
                throw;
            }
        }

        private PointGrid(
            Size pointResolution,
            ComplexArea viewPort,
            Stream pointStream)
        {
            PointResolution = pointResolution;
            ViewPort = viewPort;
            _pointStream = pointStream;
            _pointsPosition = pointStream.Position;
        }

        public void Dispose() => _pointStream.Dispose();

        public IEnumerator<PointRow> GetEnumerator()
        {
            _pointStream.Position = _pointsPosition;
            using (var reader = new BinaryReader(_pointStream, Encoding.ASCII, leaveOpen: true))
            {
                var rowSegments = new List<(bool inSet, int length)>();
                int totalLength = 0;
                int y = 0;
                while (_pointStream.Position < _pointStream.Length)
                {
                    var packedLength = reader.ReadInt32();
                    var inSet = packedLength > 0;

                    var length = inSet ? packedLength : -packedLength;

                    var segment = (inSet, length);
                    rowSegments.Add(segment);
                    totalLength += length;

                    if (totalLength == PointResolution.Width)
                    {
                        yield return new PointRow(PointResolution.Width, y, rowSegments);
                        rowSegments.Clear();
                        totalLength = 0;
                        y++;
                    }
                }
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

        public static void Write(
            string filePath,
            Size pointResolution,
            ComplexArea viewPort,
            IEnumerable<bool> pointsInSet)
        {
            using (var stream = new FileStream(filePath, FileMode.Create))
            using (var writer = new BinaryWriter(stream, Encoding.ASCII))
            {
                writer.Write(HeaderText);
                writer.WriteSize(pointResolution);
                writer.WriteComplexArea(viewPort);

                foreach (var row in pointsInSet.Batch(pointResolution.Width))
                {
                    int x = 0;
                    bool inSet = false;
                    int length = 0;
                    foreach (var point in row)
                    {
                        if (length == 0)
                        {
                            inSet = point;
                            length = 1;
                        }
                        else if (point == inSet)
                        {
                            length++;
                        }
                        else
                        {
                            writer.Write(inSet ? length : -length);

                            inSet = point;
                            length = 1;
                        }

                        x++;
                    }

                    writer.Write(inSet ? length : -length);
                }
            }
        }
    }
}
