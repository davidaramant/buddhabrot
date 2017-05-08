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

        public IEnumerable<Point> GetAreaLocations() => GetAreas().Select(ea => ea.GridLocation);

        public IEnumerable<PointPair> GetPointPairs()
        {
            float realIncrement = ViewPort.RealRange.Magnitude / (GridResolution.Width - 1);
            float imagIncrement = ViewPort.ImagRange.Magnitude / (GridResolution.Height - 1);

            float GetRealValue(int x) => ViewPort.RealRange.InclusiveMin + x * realIncrement;
            float GetImagValue(int y) => ViewPort.ImagRange.InclusiveMin + y * imagIncrement;

            FComplex GetPoint(Point location, Corners corner)
            {
                switch (corner)
                {
                    case Corners.BottomLeft:
                        return new FComplex(
                            GetRealValue(location.X),
                            GetImagValue(location.Y));
                    case Corners.BottomRight:
                        return new FComplex(
                            GetRealValue(location.X + 1),
                            GetImagValue(location.Y));
                    case Corners.TopLeft:
                        return new FComplex(
                            GetRealValue(location.X),
                            GetImagValue(location.Y + 1));
                    case Corners.TopRight:
                        return new FComplex(
                            GetRealValue(location.X + 1),
                            GetImagValue(location.Y + 1));
                    default:
                        throw new ArgumentException();
                }
            };

            foreach (var edge in GetAreas())
            {
                foreach (var cornerInSet in edge.GetCornersInSet())
                {
                    foreach (var cornerNotInSet in edge.GetCornersNotInSet())
                    {
                        yield return new PointPair(
                            inSet: GetPoint(edge.GridLocation, cornerInSet), 
                            notInSet: GetPoint(edge.GridLocation, cornerNotInSet));
                    }
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
