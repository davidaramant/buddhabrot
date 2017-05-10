using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using Buddhabrot.Core;
using Buddhabrot.Extensions;
using log4net;

namespace Buddhabrot.Edges
{
    public sealed class EdgeAreas : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(nameof(EdgeAreas));

        private const string HeaderText = "Edge Areas V2.00";
        // A sixteen byte magic header value
        private static readonly byte[] MagicBytes = Encoding.ASCII.GetBytes(HeaderText);

        public Size GridResolution { get; }
        public ComplexArea ViewPort { get; }
        public int AreaCount { get; }

        /// <summary>
        /// Gets the count of point pairs that cross the set boundary.
        /// </summary>
        public int IntersectionCount { get; }

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
                        throw new InvalidOperationException($"Unsupported edges file format: {filePath}");

                    var size = reader.ReadSize();
                    var viewPort = reader.ReadComplexArea();
                    var areaCount = reader.ReadInt32();
                    var intersectionCount = reader.ReadInt32();
                    Log.Info($"Loaded edges with resolution ({size.Width:N0}x{size.Height:N0}), " +
                             $"view port {viewPort}, {areaCount:N0} edge areas, and {intersectionCount:N0} intersections.");
                    return new EdgeAreas(size, viewPort, areaCount, intersectionCount, stream);
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
            int intersectionCount,
            Stream areaStream)
        {
            GridResolution = gridResolution;
            ViewPort = viewPort;
            AreaCount = areaCount;
            IntersectionCount = intersectionCount;
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

        public IEnumerable<EdgeSpan> GetEdgeSpans()
        {
            var cornerCalculator = new CornerCalculator(GridResolution, ViewPort);

            foreach (var edge in GetAreas())
            {
                foreach (var cornerInSet in edge.GetCornersInSet())
                {
                    foreach (var cornerNotInSet in edge.GetCornersNotInSet())
                    {
                        yield return new EdgeSpan(
                            inSet: cornerCalculator.GetCorner(edge.GridLocation, cornerInSet),
                            notInSet: cornerCalculator.GetCorner(edge.GridLocation, cornerNotInSet));
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
                    stream.Position += 2 * sizeof(int);

                    int areaCount = 0;
                    int intersectionCount = 0;
                    foreach (var area in areas)
                    {
                        switch (area.GetCornersInSet().Count())
                        {
                            case 1:
                            case 3:
                                intersectionCount += 3;
                                break;
                            case 2:
                                intersectionCount += 4;
                                break;
                            default:
                                throw new InvalidOperationException("Unexpected number of corners: " + area);
                        }

                        areaCount++;
                        writer.WriteEdgeAreaLegacy(area);
                    }
                    Log.Info($"Wrote {areaCount:N0} edge areas ({intersectionCount} intersections).");

                    stream.Position = countPosition;
                    writer.Write(areaCount);
                    writer.Write(intersectionCount);
                }
            }
        }
    }
}
